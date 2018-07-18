using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using AForge.Video;
using AForge.Video.DirectShow;

namespace Camera
{
    public partial class Form1 : Form
    {
        //视频输入设备（摄像头）的集合
        public FilterInfoCollection Cameras = null;

        //本程序使用的那个摄像头
        public VideoCaptureDevice Cam = null;

        //摄像头信息
        private FilterInfo filterInfo = null;
        private bool isMirror;
        /// <summary>
        /// 镜像
        /// </summary>
        public bool IsMirror
        {
            get
            {
                return isMirror;
            }

            set
            {
                isMirror = value;
            }
        }

        ///<summary>
        ///窗体的构造函数：
        /// Load事件用于在加载窗体时获取摄像头设备
        /// FormClosed事件用于在直接关闭窗体时关闭摄像头，释放资源
        ///</summary>
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.FormClosed += Form1_FormClosed;
        }

        ///<summary>
        ///窗体构造函数的Load事件处理程序
        ///用于获取摄像头设备
        ///</summary>
        ///<paramname="sender"></param>
        ///<paramname="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            btn1.Enabled = false;
            try
            {
                //1、获取并枚举所有摄像头设备
                Cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                //2、判断设备个数，选择某一设备
                if (Cameras.Count > 0)
                {
                    listBox1.DataSource = Cameras;
                    listBox1.DisplayMember = "Name";
                    listBox1.ValueMember = "MonikerString";
                    listBox1.SelectedValueChanged += ListBox1_SelectedValueChanged;

                    btn1.Enabled = true;
                    filterInfo = (FilterInfo)listBox1.SelectedItem;
                    Cam = new VideoCaptureDevice(filterInfo.MonikerString);
                    Cam.NewFrame += Cam_NewFrame;
                    Cam.VideoSourceError += Cam_VideoSourceError;
                    Cam.Start();
                }
                else
                {
                    MessageBox.Show("未找到视频输入设备！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        /// <summary>
        /// listBox选择项改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            //关闭老设备。
            if (Cam != null)
            {
                Cam.SignalToStop();
                //Cam.Stop();
                btn1.Enabled = false;
                btn1.Text = "关闭摄像头";
                pictureBox1.Image = null;
            }
            //开始新设备。
            btn1.Enabled = true;
            filterInfo = (FilterInfo)listBox1.SelectedItem;
            Cam = new VideoCaptureDevice(filterInfo.MonikerString);
            Cam.NewFrame += Cam_NewFrame;
            Cam.VideoSourceError += Cam_VideoSourceError;

            Cam.Start();
        }

        /// <summary>
        /// 错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void Cam_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            if (eventArgs.Description != "正在中止线程。")//排除调用Stop()的错误提示！。
            {
                //MessageBox.Show(filterInfo.Name + " 错误：\n" + eventArgs.Description + " 请选择其他设备。");
                BeginInvoke(new Action(() =>
                {
                    btn1.Enabled = false;
                    pictureBox1.Image = null;
                    Cam.SignalToStop();
                    //Cam.Stop();//不能停止，所有用SigalToStop();
                    MessageBox.Show("设备：\"" + filterInfo.Name + "\"无法使用，请选择其他设备。");
                }));
            }
        }

        ///<summary>
        ///摄像头设备Cam的NewFrame事件处理程序
        ///用于实时显示捕获的视频流
        ///</summary>
        ///<paramname="sender"></param>
        ///<paramname="eventArgs"></param>
        private void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (eventArgs.Frame.Clone() != null)
            {
                Bitmap image = (Bitmap)eventArgs.Frame.Clone();
                if (IsMirror)
                {
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                pictureBox1.Image = image;
            }
            else
            {
                btn1.Enabled = false;
                pictureBox1.Image = null;
            }
        }



        ///<summary>
        ///在关闭窗体的事件处理程序中，释放摄像头
        ///</summary>
        ///<paramname="sender"></param>
        ///<paramname="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Cam != null && Cam.IsRunning)
            {
                Cam.Stop();
            }
        }

        ///<summary>
        ///点击按钮的事件处理程序
        ///用于控制摄像头的开启、关闭
        ///</summary>
        ///<paramname="sender"></param>
        ///<paramname="e"></param>
        private void btn1_Click(object sender, EventArgs e)
        {
            if (Cam.IsRunning)
            {
                Cam.Stop();
                pictureBox1.Image = null;
                btn1.Text = "开启摄像头";
            }
            else
            {
                Cam.Start();
                btn1.Text = "关闭摄像头";
            }
        }

        private void btnMirror_Click(object sender, EventArgs e)
        {

            IsMirror = IsMirror ? false : true;
            if (IsMirror)
            {
                btnMirror.Text = "镜像关";
            }
            else
            {
                btnMirror.Text = "镜像开";
            }
        }
    }
}
