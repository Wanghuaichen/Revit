
///Author:danseshi
///Email:danseshi@yahoo.com.cn
///Bolg:http://blog.csdn.net/danseshi/
///Date:2008.7.12


using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using MouseHook.VLHooker;

namespace MouseHook
{



    public partial class Form1 : Form
    {
        Hooker Hooker = new Hooker();
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ��װ����
        /// </summary>
        private void StartHook()
        {
            Hooker.Start();

            //if (Hooker.hMouseHook == 0)
            //{
            //    Hooker.hMouseHook = Hooker.SetWindowsHookEx(Hooker.WH_MOUSE_LL, Hooker.MouseHookProcedure, Hooker.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            //    if (Hooker.hMouseHook == 0)
            //    {//������ù���ʧ��. 

            //        this.StopHook();
            //        MessageBox.Show("Set windows hook failed!");
            //    }
            //}
        }

        /// <summary>
        /// ж�ع���
        /// </summary>
        private void StopHook()
        {
            Hooker.Stop();

            //bool stop = true;
            //if (hMouseHook != 0)
            //{
            //    stop = UnhookWindowsHookEx(hMouseHook);
            //    hMouseHook = 0;

            //    if (!stop)
            //    {//ж�ع���ʧ��

            //        MessageBox.Show("Unhook failed!");
            //    }
            //}
        }

        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                //�Ѳ���lParam���ڴ���ָ�������ת��ΪMOUSEHOOKSTRUCT�ṹ
                MOUSEHOOKSTRUCT mouse = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));//���
                //���Ϊ�˿�����λ��
                this.Text = "MousePosition:" + mouse.pt.ToString();
                if (wParam == Hooker.WM_RBUTTONDOWN || wParam == Hooker.WM_RBUTTONUP)
                { //��갴�»����ͷ�ʱ��ػ�

                    if (newTaskBarRect.Contains(mouse.pt))
                    { //��������������ķ�Χ��

                        //�������1���������Ϣ�������Ϣ����Ϊֹ�����ٴ��ݡ�
                        //�������0�����CallNextHookEx��������Ϣ����������Ӽ������´��ݣ�Ҳ���Ǵ�����Ϣ�����Ľ�����
                        return 1;
                    }
                }
            }
            return Hooker.CallNextHookEx(Hooker.hMouseHook, nCode, wParam, lParam);
        }


        #region Events

        Rectangle newTaskBarRect = new Rectangle();
        private void Form1_Load(object sender, EventArgs e)
        {
            Hooker.MouseHookProcedure = new Hooker.HookProc(MouseHookProc);

            //taskBarHandleΪ���ص��������ľ��
            //Shell_TrayWndΪ������������
            int taskBarHandle = Hooker.FindWindow("Shell_TrayWnd", null);

            //���������������
            //��һ��Ҫע�⣬��������ʱ��taskBarRect�������Ǵ��ڵ����ϽǺ����½ǵ���Ļ����
            //����˵taskBarRect.Width��taskBarRect.Height���������Ļ���Ͻǣ�0��0������ֵ
            //����c#��Rectangle�ṹ�ǲ�ͬ��

            //�����������ľ�������
            Rectangle taskBarRect = new Rectangle();
            Hooker.GetWindowRect(taskBarHandle, ref taskBarRect);
            this.richTextBox1.Text = "taskBarRect.Location:" + taskBarRect.Location.ToString() + "\n";
            this.richTextBox1.Text += "taskBarRect.Size:" + taskBarRect.Size.ToString() + "\n\n";

            //����һ��c#�е�Rectangle�ṹ
            newTaskBarRect = new Rectangle(
            taskBarRect.X,
            taskBarRect.Y,
            taskBarRect.Width - taskBarRect.X,
            taskBarRect.Height - taskBarRect.Y
            );

            this.richTextBox1.Text += "newTaskBarRect.Location:" + newTaskBarRect.Location.ToString() + "\n";
            this.richTextBox1.Text += "newTaskBarRect.Size:" + newTaskBarRect.Size.ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopHook();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var parent = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            var handle = Hooker.FindWindowEx(parent, IntPtr.Zero, null, "��װ����");


            //this.StartHook();
            //this.button1.Enabled = false;
            //this.button2.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.StopHook();
            this.button1.Enabled = true;
            this.button2.Enabled = false;
        }


        #endregion


    }
}