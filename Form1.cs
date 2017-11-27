using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCO
{
    public partial class Form1 : Form
    {
        MainProgram m_program = new MainProgram();
        int m_interfaceState = (int)States.None;
        public Form1()
        {
            InitializeComponent();
            handleProgramState();

            m_program.TimerTick += (s, e) =>
            {
                // change the display time
                string time = m_program.getTimeString();
                string desc = m_program.getDescString();
                label1.Text = time;
                label2.Text = desc;

                // get the program's current state
                handleProgramState();

                // send this to the front if idle state is entered
                if (m_interfaceState == (int)States.Idle)
                {
                    this.TopMost = true;
                    this.TopMost = false;
                    this.DesktopLocation = new Point(
                        (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2,
                        (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);
                }
            };
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            m_program.onClickButton();
            handleProgramState();
        }

        private void handleProgramState()
        {
            int internalState = m_program.getState();
            bool neverTimeout = m_program.getTimeoutFlag();

            if (neverTimeout == true &&
                internalState != (int)States.Pause)
            {
                if (m_interfaceState == (int)States.NeverTimeOut)
                    return;

                m_interfaceState = (int)States.NeverTimeOut;
                timerNeverTimeout();
            }
            else
            {
                if (m_interfaceState == internalState)
                    return;
                m_interfaceState = internalState;
            
                switch (m_interfaceState)
                {
                    case (int)States.Active:       timerActive();       break;
                    case (int)States.Idle:         timerIdle();         break;
                    case (int)States.TimeOut:      timerTimeout();      break;
                    case (int)States.Pause:        timerPause();        break;
                    case (int)States.None: break;
                    default: break;
                }
            }
        }

        private void timerActive()
        {
            Console.WriteLine("Interface State now ACTIVE");
            pictureBox1.Image = Properties.Resources.buttonActive;

        }
        private void timerIdle()
        {
            Console.Beep(783, 300);
            Console.Beep(783, 300);
            Console.WriteLine("Interface State now IDLE");
            pictureBox1.Image = Properties.Resources.buttonIdle;
        }
        private void timerTimeout()
        {
            Console.WriteLine("Interface State now TIMEOUT");
            pictureBox1.Image = Properties.Resources.buttonTimeout;
        }
        private void timerPause()
        {
            Console.WriteLine("Interface State now PAUSE");
            pictureBox1.Image = Properties.Resources.buttonPause;
        }

        private void timerNeverTimeout()
        {
            this.TopMost = true;
            Console.WriteLine("Interface State now NEVERTIMEOUT");
            pictureBox1.Image = Properties.Resources.buttonNeverTimeout;
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            int keyCode = e.KeyValue;

            switch (keyCode)
            {
                case (int)Keys.Escape:
                    {
                        m_program.cancelPendingTime();
                        
                        handleProgramState();

                        Console.WriteLine("Escape Pressed - Timer is paused and time since last log has been removed");
                        break;
                    }
                case (int)Keys.Space:
                    {
                        m_program.onClickButton();
                        
                        handleProgramState();
                        Console.WriteLine("Space Pressed - Timer paused");
                        break;
                    }
                case (int)Keys.F1:
                    {
                        m_program.toggleSustainMode();
                        bool willNeverTimeout = m_program.getTimeoutFlag();

                        if (willNeverTimeout == true)
                            Console.WriteLine("F1 Pressed - NeverTimeout Enabled  - Timer will not stop counting from inactivity");
                        else
                            Console.WriteLine("F1 Pressed - NeverTimeout Disabled - Timer will stop counting from inactivity");

                        handleProgramState();
                        break;
                    }
                case (int)Keys.Enter:
                    {
                        m_program.logTime();
                        break;
                    }
                case (int)Keys.D0: m_program.setActiveLog(PrefsData.PrefLogIndex.Log01); break;
                case (int)Keys.D1: m_program.setActiveLog(PrefsData.PrefLogIndex.Log02); break;
                case (int)Keys.D2: m_program.setActiveLog(PrefsData.PrefLogIndex.Log03); break;
                default: break;
            }
            
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        private void startStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_program.onClickButton();
            handleProgramState();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            m_program.onClose();
            base.OnFormClosing(e);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogMessage message = new DialogMessage("Helpful Information", 
                "With the apllication in Focus:" +
                "\n    Press F1 to toggle Never-Time-Out mode" +
                "\n    Press Escape to remove all time accumulated since the last log" +
                "\n    Press Space to change mode as if the start/pause button were pressed" +
                "\n    Press Enter to write a log entry" +
                "\n" +
                "\nCurrently the only way to change time intervals is to change the values in the" +
                "\nconfig.prefs file that's created on close and when a log profile is selected." +
                "\n" +
                "\n" +
                "\n" +
                "\n"
                );

            message.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogMessage message = new DialogMessage("TCO v" + Environment.getVersion(), 
                "TCO is a lightweight non-invasive" +
                "\nbackground time-logging application." +
                "\n" +
                "\n    Never worry about leaving your" + 
                "\n    timer running or losing time for" +
                "\n    not starting after a break." +
                "\n" +
                "\n    TCO will automatically stop" +
                "\n    counting time when you leave" +
                "\n    and start counting again when" +
                "\n    you return.\n\n\n" +
                "© Gerald Coggins 2017"
                );
            message.Show();
        }

        private void sheetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrefsData prefData = m_program.getPrefData();

            // user preferences are updated directly from this dialog
            DialogSheetsWindow message = new DialogSheetsWindow(prefData);
            DialogResult result = message.ShowDialog();

            if (result == DialogResult.OK)
            {
                m_program.setActiveLog(message.m_selectedLog);
            }
        }
    }
}
