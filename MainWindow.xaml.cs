using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace bones_v2
{

public partial class MainWindow : Window
    {
        private static int maxURLs = 20;
        private static int selectedItem = 0;
        private static int itemHeight = 30;
        private const int WH_MOUSE_LL = 14;
        private const int WM_MBUTTONDOWN = 0x0207;
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static ListBox aStaticListBox;

        public MainWindow()
        {

            InitializeComponent();
            _hookID = SetHook(_proc);
            aStaticListBox = this.aListBox;
            for (int i = 0; i < maxURLs; i++)
            {
                aListBox.Items.Insert(0, "url " + i.ToString());
            }
            aListBox.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MBUTTONDOWN)
            {
                // Here you can add code to show your ListBox
                // Toggle visibility
                aStaticListBox.Visibility = aStaticListBox.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.BringToFront(); // Custom method to bring window to foreground
                   // aStaticListBox.Visibility = Visibility.Visible;  //.aListBox.Visibility = Visibility.Visible;
                    mainWindow?.aListBox.Focus(); // Set focus if needed
                });

            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Your implementation here
        }

        private static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        return (T)child;

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }

        private void BringToFront()
        {

            SetForegroundWindow(new WindowInteropHelper(this).Handle);

        }

        private void ScrollDownOneRow()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(aListBox); //GetScrollViewer(aListBox);
            if (scrollViewer != null)
            {

                /*        double an_offset = scrollViewer.VerticalOffset;
                        //                scrollViewer.ScrollToVerticalOffset(an_offset - itemHeight);
                        int new_offset = (int)an_offset - 1;
                        if (new_offset < 0)
                        {
                            new_offset = 0;
                        }
                */
                scrollViewer.LineDown();
                //scrollViewer.ScrollToVerticalOffset(new_offset);
                SetSelection();
            }
        }

        private void ScrollUpOneRow()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(aListBox); //GetScrollViewer(aListBox);
            if (scrollViewer != null)
            {

                /*          double an_offset = scrollViewer.VerticalOffset;
                          //                scrollViewer.ScrollToVerticalOffset(an_offset - itemHeight);
                          int new_offset = (int) an_offset + 1;
                          if (new_offset >20) {
                              new_offset = 20;
                          }
                */
                scrollViewer.LineUp();
                //scrollViewer.ScrollToVerticalOffset(new_offset);
                SetSelection();
            }
        }


        private void aListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ScrollDownOneRow();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ScrollUpOneRow();
                e.Handled = true;
            }
            else if (e.Key == Key.Z)
            {
                ScrollDownOneRow();
                e.Handled = true;
            }
            else if (e.Key == Key.X)
            {
                ScrollUpOneRow();
                e.Handled = true;
            }
        }

        private void aListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (aListBox.Items.Count == 0) return;

            // Assuming uniform item height and enough items to fill the ListBox's view
            // var visibleItemCount = CalculateVisibleItemCount();
            // var middleIndex = visibleItemCount / 2;
            var scrollViewer = FindVisualChild<ScrollViewer>(aListBox);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }

            int sIndex = maxURLs - 3;
            int vOffset = 10;
            //aListBox.SelectedIndex = vOffset + 2;
            scrollViewer.ScrollToVerticalOffset(vOffset);
            scrollViewer.UpdateLayout();
            double an_offset = scrollViewer.VerticalOffset;

        



            Dispatcher.BeginInvoke((Action)(() =>
            {
                //scrollViewer.ScrollToVerticalOffset(vOffset);
                // Any other code that needs to run after the scroll
                SetSelection();
            }), DispatcherPriority.Render);
            aListBox.Focus();
        }

        private void SetSelection()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(aListBox); //GetScrollViewer(aListBox);
            double an_offset = scrollViewer.VerticalOffset;
            //scrollViewer.ScrollToVerticalOffset(an_offset);
            aListBox.SelectedIndex = (int)an_offset + 1;
            aListBox.Focus();

        }
        private int CalculateVisibleItemCount()
        {
            // This is a simplified calculation assuming uniform item heights
            // and that the ListBox's height is set and items are already populated.
            // You need to replace 'itemHeight' with the actual height of your items.
            var itemHeight = 30; // Replace with your item height
            var listBoxHeight = aListBox.ActualHeight;

            return (int)(listBoxHeight / itemHeight);
        }


        private void aListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {
            var scrollViewer = FindVisualChild<ScrollViewer>(aListBox); //GetScrollViewer(aListBox);
  

            double an_offset = scrollViewer.VerticalOffset;
            string debug_msg = "Vertical Offset: " + an_offset.ToString();
            System.Diagnostics.Debug.WriteLine(debug_msg);
        }

        private void ScrollSelectedItemToCenter()
        {
         /*   if (aListBox.SelectedItem == null) return;

            int index = aListBox.Items.IndexOf(aListBox.SelectedItem);

            // Calculate the new offset to bring the selected item to the center
            var listBoxItem = (ListBoxItem)aListBox.ItemContainerGenerator.ContainerFromItem(aListBox.SelectedItem);
            if (listBoxItem != null)
            {
                var scrollViewer = FindVisualChild<ScrollViewer>(aListBox);
                if (scrollViewer != null)
                {
                    double newOffset = listBoxItem.TranslatePoint(new Point(), aListBox).Y - (aListBox.ActualHeight - listBoxItem.ActualHeight) / 2;
                    scrollViewer.ScrollToVerticalOffset(newOffset);
                }
            }
         */
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Your code to handle the scroll changed event
            SetSelection();
        }

    }
}
