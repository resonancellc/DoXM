﻿using DoXM_Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DoXM_Client.Services
{
    public class CMD
    {
        private static ConcurrentDictionary<string, CMD> Sessions { get; set; } = new ConcurrentDictionary<string, CMD>();
        private System.Timers.Timer ProcessIdleTimeout { get; set; }
        public static CMD GetCurrent(string connectionID)
        {
            if (Sessions.ContainsKey(connectionID))
            {
                var bash = Sessions[connectionID];
                bash.ProcessIdleTimeout.Stop();
                bash.ProcessIdleTimeout.Start();
                return bash;
            }
            else
            {
                var cmd = new CMD();
                cmd.ProcessIdleTimeout = new System.Timers.Timer(600000); // 10 minutes.
                cmd.ProcessIdleTimeout.AutoReset = false;
                cmd.ProcessIdleTimeout.Elapsed += (sender, args) =>
                {
                    CMD outResult;
                    while (!Sessions.TryRemove(connectionID, out outResult))
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    outResult.CMDProc.Kill();
                };
                while (!Sessions.TryAdd(connectionID, cmd))
                {
                    System.Threading.Thread.Sleep(1000);
                }
                cmd.ProcessIdleTimeout.Start();
                return cmd;
            }
        }
        private Process CMDProc { get; }

        private CMD()
        {
            var psi = new ProcessStartInfo("cmd.exe");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "RunAs";
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;

            CMDProc = new Process();
            CMDProc.StartInfo = psi;
            CMDProc.ErrorDataReceived += CMDProc_ErrorDataReceived;
            CMDProc.OutputDataReceived += CMDProc_OutputDataReceived;

            CMDProc.Start();

            CMDProc.BeginErrorReadLine();
            CMDProc.BeginOutputReadLine();
        }

        private void CMDProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data?.Contains(LastInputID) == true)
            {
                OutputDone = true;
            }
            else if (!OutputDone)
            {
                StandardOut += e.Data + Environment.NewLine;
            }

        }

        private void CMDProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data != null)
            {
                ErrorOut += e.Data + Environment.NewLine;
            }
        }

        public GenericCommandResult WriteInput(string input, string commandID)
        {
            StandardOut = "";
            ErrorOut = "";
            lock (CMDProc)
            {
                LastInputID = commandID;
                OutputDone = false;
                CMDProc.StandardInput.WriteLine(input);
                CMDProc.StandardInput.WriteLine("echo " + commandID);
                while (!OutputDone)
                {
                    Thread.Sleep(1);
                }
            }
            return new GenericCommandResult()
            {
                CommandContextID = commandID,
                MachineID = Utilities.GetConnectionInfo().MachineID,
                CommandType = "CMD",
                StandardOutput = StandardOut,
                ErrorOutput = ErrorOut
            };
        }


        private string LastInputID { get; set; }
        private bool OutputDone { get; set; }
        private string StandardOut { get; set; }
        private string ErrorOut { get; set; }

    }
}
