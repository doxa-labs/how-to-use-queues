using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace HowToUseQueues
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 1001; i++)
            {
                Enqueue(i.ToString() + "jobs count: " + _jobs.ToString());
            }
        }

        private Queue<string> _jobs = new Queue<string>();
        private bool _delegateQueuedOrRunning = false;

        public void Enqueue(string job)
        {
            lock (_jobs)
            {
                _jobs.Enqueue(job);
                if (!_delegateQueuedOrRunning)
                {
                    _delegateQueuedOrRunning = true;
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                }
            }
        }

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                string item;
                lock (_jobs)
                {
                    if (_jobs.Count == 0)
                    {
                        _delegateQueuedOrRunning = false;
                        break;
                    }

                    item = _jobs.Dequeue();
                }

                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        //do job
                        logsRichTextBox.AppendText("text: " + item);
                        logsRichTextBox.ScrollToEnd();
                    });
                }
                catch
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    throw;
                }
            }
        }
    }
}
