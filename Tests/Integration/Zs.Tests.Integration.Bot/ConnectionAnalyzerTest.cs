using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Zs.Common.Enums;
using Zs.Common.Services.Connection;

namespace Zs.Tests.Integration.Bot
{
    [TestClass]
    public class ConnectionAnalyzerTest
    {
        private ConnectionStatus _changedStatus;

        public ConnectionAnalyzerTest()
        {
            
        }

        [TestMethod]
        public async Task LoopWork_Test()
        {
            try
            {
                 var analyser = new ConnectionAnalyser("https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                analyser.InitializeProxy("123.123.123.123:12345", "userName", "password");
                analyser.ConnectionStatusChanged += StatusChanged;
                
                Exception exception = null;
                await Task.Run(async () =>
                {
                    analyser.Start(100, 2000); 
                    await Task.Delay(3000);
                    _changedStatus = ConnectionStatus.Undefined;
                    for (int i = 0; i < 100; i++)
                    {
                        await Task.Delay(500);
                        if (_changedStatus != ConnectionStatus.Undefined)
                        {
                            analyser.Stop();
                            analyser.ConnectionStatusChanged -= StatusChanged;
                            break;
                        }
                    }
                }).ContinueWith(task => exception = task.Exception);

                if (exception != null)
                    throw exception;

                Assert.IsTrue(_changedStatus != ConnectionStatus.Undefined);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void PingHost_Test()
        {
            try
            {
                Assert.IsTrue(ConnectionAnalyser.PingHost("localhost"));
                Assert.IsTrue(ConnectionAnalyser.PingHost("https://vk.com/"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void StatusChanged(ConnectionStatus newConnectionStatus)
        {
            _changedStatus = newConnectionStatus;
        }
    }
}
