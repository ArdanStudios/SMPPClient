#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ArdanStudios.Common.Utilities
{
    /// <summary> Maintains a synchronous timer event </summary>
    public class SynchronousTimer : IDisposable
    {
        #region Delegates
        
        /// <summary> Called by the timer </summary>
        public delegate void SynchronousTimerHandler(object state, SynchronousTimer theTimer);
        
        #endregion
        
        #region Public Properties
        
        /// <summary> Set when the class is shutting down </summary>
        public bool ShuttingDown = false;
        
        #endregion
        
        #region Private Properties
        
        /// <summary> Flag that determines whether this instance has been disposed or not yet </summary>
        private bool Disposed = false;
        
        /// <summary> Thread waits on this event on the timer interval </summary>
        private ManualResetEvent TimerEventInterval = new ManualResetEvent(false);
        
        /// <summary> Thread waits on this event for thread to shutdown </summary>
        private AutoResetEvent TimerWaitShutdown = new AutoResetEvent(false);
        
        /// <summary> The interval the timer should fire </summary>
        private int TimerInterval = 1000;
        
        /// <summary> State to be passed in </summary>
        private object TimerState = null;
        
        /// <summary> Handle to the timer thread </summary>
        private Thread TimerThread = null;
        
        /// <summary> Handle to the timer function </summary>
        private SynchronousTimerHandler TimerMethod = null;
        
        #endregion
        
        #region Constructor
        
        /// <summary> Constructor </summary>
        /// <param name="timerMethod"></param>
        /// <param name="timerInterval"></param>
        /// <param name="timerState"></param>
        /// <param name="timerName"></param>
        public SynchronousTimer(SynchronousTimerHandler timerMethod, object timerState, int timerInterval, string timerName = null)
        {
            TimerMethod = timerMethod;
            TimerState = timerState;
            TimerInterval = timerInterval;
            
            TimerThread = new Thread(new ThreadStart(PerformTimerEvent));
            TimerThread.Name = (timerName == null) ? "SynchronousTimer" : string.Format("SynchronousTimer-{0}", timerName);
            TimerThread.Start();
        }
        
        /// <summary> Constructor </summary>
        /// <param name="timerMethod"></param>
        /// <param name="timerInterval"></param>
        /// <param name="timerState"></param>
        /// <param name="threadPriority"></param>
        /// <param name="timerName"></param>
        public SynchronousTimer(SynchronousTimerHandler timerMethod, object timerState, int timerInterval, ThreadPriority threadPriority, string timerName = null)
        {
            TimerMethod = timerMethod;
            TimerState = timerState;
            TimerInterval = timerInterval;
            
            TimerThread = new Thread(new ThreadStart(PerformTimerEvent));
            TimerThread.Name = (timerName == null) ? "SynchronousTimer" : string.Format("SynchronousTimer-{0}", timerName);
            TimerThread.Priority = threadPriority;
            TimerThread.Start();
        }

        /// <summary> Constructor that will set off the timer every minute on the minute </summary>
        /// <param name="timerMethod"></param>
        /// <param name="timerState"></param>
        /// <param name="timerName"></param>
        public SynchronousTimer(SynchronousTimerHandler timerMethod, object timerState, string timerName = null)
        {
            TimerMethod = timerMethod;
            TimerState = timerState;
            TimerInterval = 60000;
            
            TimerThread = new Thread(new ThreadStart(PerformMinuteTimerEvent));
            TimerThread.Name = (timerName == null) ? "SynchronousTimer" : string.Format("SynchronousTimer-{0}", timerName);
            TimerThread.Start();
        }
        
        /// <summary> Dispose </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary> Dispose </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                // Note disposing has been done.
                Disposed = true;

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Wake up the thread so it can shutdown
                    TimerEventInterval.Set();
                    
                    // Wait for the thread to shutdown
                    TimerWaitShutdown.WaitOne(10000);
                }
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary> Called to signal the timer to wake the thread up early</summary>
        public void SignalTimer()
        {
            // Wake up the thread so it can shutdown
            TimerEventInterval.Set();
        }

        #endregion
        
        #region Private Methods
        
        /// <summary> Called to implement the timer </summary>
        private void PerformTimerEvent()
        {
            for (;;)
            {
                try
                {
                    // Wait here for the timer to expire
                    if (TimerEventInterval.WaitOne(TimerInterval))
                    {
                        if (Disposed == true)
                        {
                            // Tell dispose we are done
                            TimerWaitShutdown.Set();
                        
                            // We are shutting down. This should always expire
                            return;
                        }

                        // Reset the event
                        TimerEventInterval.Reset();
                    }
                    
                    // Call the timer method
                    TimerMethod(TimerState, this);
                }
        
                catch
                {
                }
            }
        }

        /// <summary> Called to implement the timer every minute on the second </summary>
        private void PerformMinuteTimerEvent()
        {
            for (;;)
            {
                try
                {
                    // Try to adjust to the nearest second
                    DateTime now = DateTime.UtcNow;
                    
                    // Calculate the number of milliseconds to wait
                    int diff = (60 - now.Second) * 1000;

                    // Wait for the clock to sync
                    if (TimerEventInterval.WaitOne(diff))
                    {
                        if (Disposed == true)
                        {
                            // Tell dispose we are done
                            TimerWaitShutdown.Set();
                        
                            // We are shutting down. This should always expire
                            return;
                        }

                        // Reset the event
                        TimerEventInterval.Reset();
                    }

                    // Call the timer method
                    TimerMethod(TimerState, this);
                }
        
                catch
                {
                }
            }
        }
         
        #endregion
    }
}