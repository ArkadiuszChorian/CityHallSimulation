using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

//michal.kowalski@put.poznan.pl

namespace Lab7SCRzWdomu
{
    public partial class Form1 : Form
    {
        public class Window
        {
            public int windowIdNumber;
            static int numberOfWindows = 0;
            public int currentCase;
            public Barrier syncWindowWithClient;
            public Barrier syncWindowWithPresenter;
            bool windowFree;
            bool windowStatus;
            public List<int> finishedCaseList;
            public Thread currentWorker;

            public Window(Thread worker)
            {
                numberOfWindows++;
                windowIdNumber = numberOfWindows;
                syncWindowWithClient = new Barrier(2);
                syncWindowWithPresenter = new Barrier(2);
                currentWorker = worker;
                windowFree = false;
                windowStatus = false;
                finishedCaseList = new List<int>();
                currentWorker.Start();
                //currentWorker.Suspend();
            }

            public Window()
            {
                numberOfWindows++;
                windowIdNumber = numberOfWindows;
                syncWindowWithClient = new Barrier(2);
                syncWindowWithPresenter = new Barrier(2);
                currentWorker = null;
                windowFree = false;
                finishedCaseList = new List<int>();
            }

            public void setWindowOpen(bool status)
            {
                windowStatus = status;
            }

            public bool isWindowOpen()
            {
                return windowStatus;
            }

            public void setCurrentWorker(Thread worker)
            {
                currentWorker = worker;
            }

            public bool isWindowFree()
            {
                return windowFree;
            }

            public void setCurrentCase(int incomingCase)
            {
                currentCase = incomingCase;
            }

            public void signalizeItsDone()
            {
                syncWindowWithClient.SignalAndWait();
            }

            public void signalizeWaiting()
            {
                syncWindowWithClient.SignalAndWait();
            }

            public void signalizeReadyToShow()
            {
                syncWindowWithPresenter.SignalAndWait();
            }

            public void signalizeItsShowed()
            {
                syncWindowWithPresenter.SignalAndWait();
            }

            public void signalizeWaitingToShow()
            {
                syncWindowWithPresenter.SignalAndWait();
            }

            public void signalizeWaitingItsShowed()
            {
                syncWindowWithPresenter.SignalAndWait();
            }

            public void saveFinishedCase()
            {
                finishedCaseList.Add(currentCase);
            }

            public void changeWorker(Thread newWorker)
            {
                currentWorker.Abort();
                currentWorker = newWorker;
                currentWorker.Start();
            }

            public void closeWindow()
            {
                windowFree = false;
                setWindowOpen(false);
                //currentWorker.Suspend();
            }

            public void openWindow(Thread worker)
            {
                currentWorker = worker;
                windowFree = true;
                
            }

            public void openWindow()
            {
                windowFree = true;
                setWindowOpen(true);
                //currentWorker.Resume();
            }

            public void releaseWindow()
            {
                windowFree = true;
            }

            public void blockWindow()
            {
                windowFree = false;
            }

            public void caseProcessing()
            {
                currentCase /= 2;
            }

            public void caseSigning()
            {
                currentCase -= 10;
            }
        }

        static bool w2justClosed = false;
        static bool w3justClosed = false;

        static bool w2firstTime = true;
        static bool w3firstTime = true;

        const int numberOfWorkers = 3;
        const int initialNumberOfClients = 6;
        const int initialNumberOfActiveWorkers = 1;
        static List<Thread> clientsThreads = new List<Thread>();       
        static Semaphore blockHall = new Semaphore(0, 3);
        static Mutex blockSavingToMainList = new Mutex();
        static Mutex blockChoosingWindow = new Mutex();
        static Mutex blockCaseGenerator = new Mutex();
        static Mutex blockClientList = new Mutex();
        static Mutex blockActualizingWindow1Case = new Mutex();
        static Mutex blockActualizingWindow2Case = new Mutex();
        static Mutex blockActualizingWindow3Case = new Mutex();
        static Mutex blockOperatingOnHallSemaphore = new Mutex();
        static Random caseGenerator = new Random();
        static List<int> mainList = new List<int>();
        static Thread worker1 = new Thread(worker1DoWork);
        static Thread worker2 = new Thread(worker2DoWork);
        static Thread worker3 = new Thread(worker3DoWork);
        static Window window1 = new Window(worker1);
        static Window window2 = new Window(worker2);
        static Window window3 = new Window(worker3);
        static Window worker1Window = new Window();
        static Window worker2Window = new Window();
        static Window worker3Window = new Window();
        /*static object signalizeChangesInWindow1 = new object();
        static object signalizeChangesInWindow2 = new object();
        static object signalizeChangesInWindow3 = new object();*/
        static int[] caseS1 = new int[3];
        static int[] caseS2 = new int[3];
        static int[] caseS3 = new int[3];
        static bool closeingPending = false;
        //static int levelOfCrowd = 1;

        /*static int currentCaseWindow1 = 0;
        static int currentCaseWindow2 = 0;
        static int currentCaseWindow3 = 0;
        static Barrier syncWindow1 = new Barrier(2);
        static Barrier syncWindow2 = new Barrier(2);
        static Barrier syncWindow3 = new Barrier(2);
        static bool window1IsFree = true;
        static bool window2IsFree = false;
        static bool window3IsFree = false;
        static List<int> worker1List = new List<int>();
        static List<int> worker2List = new List<int>();
        static List<int> worker3List = new List<int>();
        static Thread worker1 = new Thread(worker1DoWork);
        static Thread worker2 = new Thread(worker2DoWork);
        static Thread worker3 = new Thread(worker3DoWork);*/

        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < initialNumberOfClients; i++)
            {
                clientsThreads.Add(new Thread(clientDoWork));
                clientsThreads.Last().Start();
            }

            worker1Window = window1;
            //worker1Window.openWindow();         

            //window2.setCurrentWorker(worker2);
            worker2Window = window2;
            //window3.setCurrentWorker(worker3);
            worker3Window = window3;

            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
            backgroundWorker3.RunWorkerAsync();

            textBoxClientTime.Text = timerClientGenerator.Interval.ToString();
        }
        //static int num = 0;
        static void clientDoWork(object parameter)
        {
            blockCaseGenerator.WaitOne();
            Random localCaseGenerator = new Random(caseGenerator.Next());           
            int clientCase = localCaseGenerator.Next(100, 199);           
            //int clientCase = num++;
            blockCaseGenerator.ReleaseMutex();

            //Thread.Sleep(caseGenerator.Next(100, 500));
            //blockOperatingOnHallSemaphore.WaitOne();
            blockHall.WaitOne();

            /*if (window1IsFree)
                syncWindow1.SignalAndWait();
            else if (window2IsFree)
                syncWindow2.SignalAndWait();
            else if (window3IsFree)
                syncWindow3.SignalAndWait();*/

            Window currentWindow = new Window();

            blockChoosingWindow.WaitOne();

            /*while (!window1.isWindowOpen() && 
                   !window2.isWindowOpen() && 
                   !window3.isWindowOpen()) { }*/

            if (window1.isWindowFree())
                currentWindow = window1;
            if (window2.isWindowFree())
                currentWindow = window2;
            if (window3.isWindowFree())
                currentWindow = window3;

            currentWindow.blockWindow();

            blockChoosingWindow.ReleaseMutex();

            Thread.Sleep(caseGenerator.Next(100, 500));
            currentWindow.setCurrentCase(clientCase);
            currentWindow.signalizeItsDone();
            currentWindow.signalizeWaiting();
            Thread.Sleep(caseGenerator.Next(100, 500));
            currentWindow.caseSigning();
            currentWindow.signalizeItsDone();
            currentWindow.signalizeWaiting();
            Thread.Sleep(caseGenerator.Next(100, 500));

            currentWindow.releaseWindow();
            try
            {
                blockHall.Release();
            }
            catch
            {

            }

            //blockOperatingOnHallSemaphore.ReleaseMutex();
        }
        
        static void worker1DoWork(object parameter)
        {
            while (true)
            {
                while (worker1Window.isWindowOpen() == false) { }
                //Thread.Sleep(100);
                //blockActualizingWindow1Case.WaitOne();               
                //Thread.Sleep(caseGenerator.Next(100, 500));             
                worker1Window.signalizeWaiting();
                //lock(signalizeChangesInWindow1)
                //Monitor.Pulse(signalizeChangesInWindow1);
                caseS1[0] = worker1Window.currentCase;
                window1.signalizeReadyToShow();
                window1.signalizeWaitingItsShowed();
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker1Window.caseProcessing();
                //lock (signalizeChangesInWindow1)
                //Monitor.Pulse(signalizeChangesInWindow1);
                caseS1[1] = worker1Window.currentCase;
                window1.signalizeReadyToShow();
                window1.signalizeWaitingItsShowed();
                worker1Window.signalizeItsDone();               
                worker1Window.signalizeWaiting();              
                //lock (signalizeChangesInWindow1)
                //Monitor.Pulse(signalizeChangesInWindow1);
                caseS1[2] = worker1Window.currentCase;
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker1Window.saveFinishedCase();             
                //blockActualizingWindow1Case.ReleaseMutex();
                window1.signalizeReadyToShow();
                window1.signalizeWaitingItsShowed();
                worker1Window.signalizeItsDone();
                //Thread.Sleep(caseGenerator.Next(100, 500));
                //lock (signalizeChangesInWindow1)
                //Monitor.Pulse(signalizeChangesInWindow1);                                                        
            }
        }
        static void worker2DoWork(object parameter)
        {
            while (true)
            {
                while (worker2Window.isWindowOpen() == false)
                {

                }
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker2Window.signalizeWaiting();
                caseS2[0] = worker2Window.currentCase;
                window2.signalizeReadyToShow();
                window2.signalizeWaitingItsShowed();
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker2Window.caseProcessing();
                caseS2[1] = worker2Window.currentCase;
                window2.signalizeReadyToShow();
                window2.signalizeWaitingItsShowed();
                worker2Window.signalizeItsDone();
                worker2Window.signalizeWaiting();
                caseS2[2] = worker2Window.currentCase;
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker2Window.saveFinishedCase();
                window2.signalizeReadyToShow();
                window2.signalizeWaitingItsShowed();
                worker2Window.signalizeItsDone();
            }
        }
        static void worker3DoWork(object parameter)
        {
            while (true)
            {
                while (worker3Window.isWindowOpen() == false)
                {

                }
                w3justClosed = false;
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker3Window.signalizeWaiting();
                caseS3[0] = worker3Window.currentCase;
                window3.signalizeReadyToShow();
                window3.signalizeWaitingItsShowed();
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker3Window.caseProcessing();
                caseS3[1] = worker3Window.currentCase;
                window3.signalizeReadyToShow();
                window3.signalizeWaitingItsShowed();
                worker3Window.signalizeItsDone();
                worker3Window.signalizeWaiting();
                caseS3[2] = worker3Window.currentCase;
                Thread.Sleep(caseGenerator.Next(100, 500));
                worker3Window.saveFinishedCase();
                window3.signalizeReadyToShow();
                window3.signalizeWaitingItsShowed();
                worker3Window.signalizeItsDone();
                
            }
        }
        
        private void timerClientGenerator_Tick(object sender, EventArgs e)
        {
            blockClientList.WaitOne();
            clientsThreads.Add(new Thread(clientDoWork));
            clientsThreads.Last().Start();
            //Thread.Sleep(5);
            blockClientList.ReleaseMutex();
        }

        private void buttonOpenUM_Click(object sender, EventArgs e)
        {
            if(labelUMOpen.Text == "Zamknięte")
            {
                labelUMOpen.Text = "Otwarte";
                labelUMOpen.BackColor = Color.LightGreen;
                buttonAddClient.Enabled = true;
                if (window1.isWindowOpen() == false)
                    window1.openWindow();
                blockHall.Release();
                //timerRefresher.Enabled = true;
            }
            /*labelUMOpen.Text = "Otwarte";
            labelUMOpen.BackColor = Color.LightGreen;
            if (window1.isWindowOpen() == false)
                window1.openWindow();
            blockHall.Release();
            //timerRefresher.Enabled = true;*/
        }

        private void timerClientsRemover_Tick(object sender, EventArgs e)
        {
            blockClientList.WaitOne();
            List<int> indexes = new List<int>();
            for (int i = 0; i < clientsThreads.Count; i++)
            {
                if (clientsThreads.ElementAt(i).ThreadState == ThreadState.Stopped)
                {
                    indexes.Add(i);
                }
            }
            for (int i = 0; i < indexes.Count; i++)
            {
                try
                {
                    clientsThreads.RemoveAt(indexes.ElementAt(i));
                }
                catch (ArgumentOutOfRangeException) { }           
            }
            indexes.Clear();
            blockClientList.ReleaseMutex();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                window1.signalizeWaitingToShow();
                //blockActualizingWindow1Case.WaitOne();
                //textBoxClientTime.Text = "X";
                //timerClientGenerator.Interval++;
                //lock (signalizeChangesInWindow1)
                //Monitor.Wait(signalizeChangesInWindow1);
                backgroundWorker1.ReportProgress(0, 1);
                window1.signalizeItsShowed();
                window1.signalizeWaitingToShow();
                //textBoxS1D1.AppendText(worker1Window.currentCase.ToString() + "\n");
                //lock (signalizeChangesInWindow1)
                //Monitor.Wait(signalizeChangesInWindow1);
                backgroundWorker1.ReportProgress(0, 2);
                window1.signalizeItsShowed();
                window1.signalizeWaitingToShow();
                //textBoxS1D2.AppendText(worker1Window.currentCase.ToString() + "\n");
                //lock (signalizeChangesInWindow1)
                //Monitor.Wait(signalizeChangesInWindow1);
                backgroundWorker1.ReportProgress(0, 3);
                window1.signalizeItsShowed();
                //blockActualizingWindow1Case.ReleaseMutex();
                //textBoxS1D3.AppendText(worker1Window.currentCase.ToString() + "\n");
            }
        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch ((int)e.UserState)
            {
                case 1:
                    textBoxS1D1.AppendText(caseS1[0].ToString() + "\n");
                    break;
                case 2:
                    textBoxS1D2.AppendText(caseS1[1].ToString() + "\n");
                    break;
                case 3:
                    textBoxS1D3.AppendText(caseS1[2].ToString() + "\n");                   
                    break;
                default:
                    break;
            }
            //timerClientGenerator.Interval++;
            //textBoxClientTime.Text = timerClientGenerator.Interval.ToString();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                window2.signalizeWaitingToShow();
                backgroundWorker2.ReportProgress(0, 1);
                window2.signalizeItsShowed();
                window2.signalizeWaitingToShow();
                backgroundWorker2.ReportProgress(0, 2);
                window2.signalizeItsShowed();
                window2.signalizeWaitingToShow();
                backgroundWorker2.ReportProgress(0, 3);
                window2.signalizeItsShowed();
            }
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch ((int)e.UserState)
            {
                case 1:
                    textBoxS2D1.AppendText(caseS2[0].ToString() + "\n");
                    break;
                case 2:
                    textBoxS2D2.AppendText(caseS2[1].ToString() + "\n");
                    break;
                case 3:
                    textBoxS2D3.AppendText(caseS2[2].ToString() + "\n");
                    break;
                default:
                    break;
            }
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                window3.signalizeWaitingToShow();
                backgroundWorker3.ReportProgress(0, 1);
                window3.signalizeItsShowed();
                window3.signalizeWaitingToShow();
                backgroundWorker3.ReportProgress(0, 2);
                window3.signalizeItsShowed();
                window3.signalizeWaitingToShow();
                backgroundWorker3.ReportProgress(0, 3);
                window3.signalizeItsShowed();
            }
        }

        private void backgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch ((int)e.UserState)
            {
                case 1:
                    textBoxS3D1.AppendText(caseS3[0].ToString() + "\n");
                    break;
                case 2:
                    textBoxS3D2.AppendText(caseS3[1].ToString() + "\n");
                    break;
                case 3:
                    textBoxS3D3.AppendText(caseS3[2].ToString() + "\n");
                    break;
                default:
                    break;
            }
        }

        private void timerRefresher_Tick(object sender, EventArgs e)
        {
            if (labelUMOpen.Text == "Otwarte")
            {
                if (clientsThreads.Count >= 22)
                {
                    //blockOperatingOnHallSemaphore.WaitOne();
                    if (window1.isWindowOpen() == false)
                    {
                        window1.openWindow();
                        //if (labelUMOpen.Text == "Otwarte")
                        blockHall.Release();
                    }

                    if (window2.isWindowOpen() == false)
                    {
                        window2.openWindow();
                        w2firstTime = true;
                        //if(labelUMOpen.Text == "Otwarte")
                        blockHall.Release();
                    }

                    if (window3.isWindowOpen() == false)
                    {
                        window3.openWindow();
                        //if (labelUMOpen.Text == "Otwarte")
                        blockHall.Release();
                    }
                    //blockOperatingOnHallSemaphore.ReleaseMutex();
                }
                else if (clientsThreads.Count >= 12 && clientsThreads.Count < 22)
                {
                    //blockOperatingOnHallSemaphore.WaitOne();
                    if (window1.isWindowOpen() == false)
                    {
                        window1.openWindow();
                        //if (labelUMOpen.Text == "Otwarte")
                        blockHall.Release();
                    }

                    if (window2.isWindowOpen() == false)
                    {
                        window2.openWindow();
                        //w2firstTime = true;
                        //if (labelUMOpen.Text == "Otwarte")
                        blockHall.Release();
                    }

                    if (window3.isWindowOpen() == true)
                    {
                        window3.closeWindow();
                        //w3justClosed = true;
                        //blockHall.WaitOne();
                    }
                    //blockOperatingOnHallSemaphore.ReleaseMutex();
                }
                else if (clientsThreads.Count > 0 && clientsThreads.Count < 12)
                {
                    //blockOperatingOnHallSemaphore.WaitOne();
                    if (window1.isWindowOpen() == false)
                    {
                        window1.openWindow();
                        //if (labelUMOpen.Text == "Otwarte")
                        blockHall.Release();
                    }

                    if (window2.isWindowOpen() == true)
                    {
                        window2.closeWindow();
                        //w2justClosed = true;
                        //blockHall.WaitOne();
                    }

                    if (window3.isWindowOpen() == true)
                    {
                        window3.closeWindow();
                        //w3justClosed = true;
                        //blockHall.WaitOne();
                    }
                    //blockOperatingOnHallSemaphore.ReleaseMutex();
                }
                else if (clientsThreads.Count == 0)
                {
                    if (window1.isWindowOpen() == true)
                    {
                        window1.closeWindow();
                        blockHall.WaitOne();
                        //if (labelUMOpen.Text == "Otwarte")
                        //blockHall.Release();
                    }

                    if (window2.isWindowOpen() == true)
                    {
                        window2.closeWindow();
                        w2justClosed = true;
                        blockHall.WaitOne();
                    }

                    if (window3.isWindowOpen() == true)
                    {
                        window3.closeWindow();
                        blockHall.WaitOne();
                    }
                }
            }           

            if (window1.isWindowOpen() == true)
            {
                textBoxS1D1.BackColor = SystemColors.Window;
                textBoxS1D2.BackColor = SystemColors.Window;
                textBoxS1D3.BackColor = SystemColors.Window;
            }else
            {
                textBoxS1D1.BackColor = SystemColors.Control;
                textBoxS1D2.BackColor = SystemColors.Control;
                textBoxS1D3.BackColor = SystemColors.Control;
            }
            if (window2.isWindowOpen() == true)
            {
                textBoxS2D1.BackColor = SystemColors.Window;
                textBoxS2D2.BackColor = SystemColors.Window;
                textBoxS2D3.BackColor = SystemColors.Window;
            }
            else
            {
                textBoxS2D1.BackColor = SystemColors.Control;
                textBoxS2D2.BackColor = SystemColors.Control;
                textBoxS2D3.BackColor = SystemColors.Control;
            }
            if (window3.isWindowOpen() == true)
            {
                textBoxS3D1.BackColor = SystemColors.Window;
                textBoxS3D2.BackColor = SystemColors.Window;
                textBoxS3D3.BackColor = SystemColors.Window;
            }
            else
            {
                textBoxS3D1.BackColor = SystemColors.Control;
                textBoxS3D2.BackColor = SystemColors.Control;
                textBoxS3D3.BackColor = SystemColors.Control;
            }
                
            textBoxNumberOfClients.Text = clientsThreads.Count.ToString();
            if (closeingPending && clientsThreads.Count == 0)
            {
                //blockOperatingOnHallSemaphore.WaitOne();
                labelUMOpen.Text = "Zamknięte";
                labelUMOpen.BackColor = Color.Red;
                blockHall.WaitOne();
                closeingPending = false;
                //blockOperatingOnHallSemaphore.ReleaseMutex();
            }
        }

        private void buttonClientTimePlus_Click(object sender, EventArgs e)
        {
            timerClientGenerator.Interval += 100;
            textBoxClientTime.Text = timerClientGenerator.Interval.ToString();
        }

        private void buttonClientTimeMinus_Click(object sender, EventArgs e)
        {
            if(timerClientGenerator.Interval > 100)
                timerClientGenerator.Interval -= 100;
            textBoxClientTime.Text = timerClientGenerator.Interval.ToString();
        }

        private void buttonAddClient_Click(object sender, EventArgs e)
        {
            blockClientList.WaitOne();
            clientsThreads.Add(new Thread(clientDoWork));
            clientsThreads.Last().Start();
            blockClientList.ReleaseMutex();
        }

        private void buttonRemoveClient_Click(object sender, EventArgs e)
        {
            if(clientsThreads.Count>=1)
            {
                blockClientList.WaitOne();
                clientsThreads.Last().Abort();
                if (clientsThreads.Count > 0)
                    clientsThreads.Remove(clientsThreads.Last());
                //clientsThreads.Last().Start();
                blockClientList.ReleaseMutex();
            }
            /*blockClientList.WaitOne();
            clientsThreads.Last().Abort();
            if(clientsThreads.Count > 0)
                clientsThreads.Remove(clientsThreads.Last());
            //clientsThreads.Last().Start();
            blockClientList.ReleaseMutex();*/
        }

        private void buttonCloseUM_Click(object sender, EventArgs e)
        {
            if (labelUMOpen.Text == "Otwarte")
            {
                labelUMOpen.Text = "Zamykanie";
                labelUMOpen.BackColor = Color.Yellow;
                timerClientGenerator.Enabled = false;
                buttonAddClient.Enabled = false;
                closeingPending = true;   
            }

            /*labelUMOpen.Text = "Zamykanie";
            labelUMOpen.BackColor = Color.Yellow;
            timerClientGenerator.Enabled = false;
            buttonAddClient.Enabled = false;
            closeingPending = true;  */          
        }

        private void buttonDisableGen_Click(object sender, EventArgs e)
        {
            timerClientGenerator.Enabled = false;
        }

        private void buttonEnableGen_Click(object sender, EventArgs e)
        {
            timerClientGenerator.Enabled = true;
        }

        private void buttonClientsStatus_Click(object sender, EventArgs e)
        {
            string text = "";

            foreach (Thread client in clientsThreads)
            {
                text += client.ThreadState.ToString();
                text += "\n";
            }
            MessageBox.Show(text);
        }
    }
}
