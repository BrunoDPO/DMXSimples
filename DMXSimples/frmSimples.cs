using System;
using System.Linq;
using System.Windows.Forms;

namespace BrunoDPO.DMX
{
	public partial class frmSimples : Form
	{
		private byte[] buffer = new byte[512];
		private DMXCommunicator dmxCommunicator = null;

		public frmSimples()
		{
			InitializeComponent();
			this.Icon = BrunoDPO.DMX.Properties.Resources.Icon;

			var portsList = DMXCommunicator.GetValidSerialPorts();
			cbxSerialPort.DataSource = new BindingSource(portsList, null);
		}

		private void TrackBar_Scroll(object sender, EventArgs e)
		{
			var trackBar = sender as TrackBar;
			var position = Convert.ToInt16(trackBar.Name.Substring(8));
			var numericUpDown = this.Controls.Find(string.Format("numericUpDown{0}", position), true).FirstOrDefault() as NumericUpDown;

			if (numericUpDown != null)
				numericUpDown.Value = trackBar.Value;
		}

		private void NumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			var numericUpDown = sender as NumericUpDown;
			var position = Convert.ToInt16(numericUpDown.Name.Substring(13));
			var trackbar = this.Controls.Find(string.Format("trackbar{0}", position), true).FirstOrDefault() as TrackBar;

			if (trackbar != null)
				trackbar.Value = Convert.ToInt32(numericUpDown.Value);

			buffer[position] = Convert.ToByte(numericUpDown.Value);
			if (dmxCommunicator != null)
				dmxCommunicator.SetByte(position - 1, Convert.ToByte(numericUpDown.Value));
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (cbxSerialPort.Items.Count == 0)
				return;

			dmxCommunicator = new DMXCommunicator(cbxSerialPort.SelectedValue.ToString());
			dmxCommunicator.SetBytes(buffer);
			dmxCommunicator.Start();

			cbxSerialPort.Enabled = !cbxSerialPort.Enabled;
			btnStart.Enabled = !btnStart.Enabled;
			btnStop.Enabled = !btnStop.Enabled;
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			if (dmxCommunicator != null && dmxCommunicator.IsActive)
				dmxCommunicator.Stop();

			cbxSerialPort.Enabled = !cbxSerialPort.Enabled;
			btnStart.Enabled = !btnStart.Enabled;
			btnStop.Enabled = !btnStop.Enabled;
		}

		private void frmSimple_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (dmxCommunicator != null && dmxCommunicator.IsActive)
			{
				dmxCommunicator.Stop();
				dmxCommunicator = null;
			}
		}
	}
}
