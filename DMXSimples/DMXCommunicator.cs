using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

//  ______                          _____   ______   _____  
// (____  \                        (____ \ (_____ \ / ___ \ 
//  ____)  ) ____ _   _ ____   ___  _   \ \ _____) ) |   | |
// |  __  ( / ___) | | |  _ \ / _ \| |   | |  ____/| |   | |
// | |__)  ) |   | |_| | | | | |_| | |__/ /| |     | |___| |
// |______/|_|    \____|_| |_|\___/|_____/ |_|      \_____/ 
//
// 01000010 01110010 01110101 01101110 01101111 01000100 01010000 01001111

namespace BrunoDPO.DMX
{
	/// <summary>
	/// This class implements the DMX Communication Protocol over a Serial Port.
	/// It is recommended for this class to work that you either buy or make a
	/// RS232 to RS485 converter.
	/// </summary>
	public class DMXCommunicator
	{
		private byte[] buffer = new byte[513];
		private bool isActive = false;
		private Thread senderThread;
		private SerialPort serialPort;

		/// <summary>
		/// Initialize a DMXCommunicator class
		/// </summary>
		/// <param name="portName">Name of the serial port as a string</param>
		/// <exception cref="Exception">If the serial port is somehow inaccessible</exception>
		public DMXCommunicator(string portName) : this(new SerialPort(portName)) { }

		/// <summary>
		/// Initialize a DMXCommunicator class
		/// </summary>
		/// <param name="port">Instance of a SerialPort</param>
		/// <exception cref="Exception">If the serial port is somehow inaccessible</exception>
		public DMXCommunicator(SerialPort port)
		{
			buffer[0] = 0; // The first byte must be a zero
			serialPort = ConfigureSerialPort(port);
		}

		/// <summary>
		/// Set a connection and try to open it to see if the port is
		/// compatible with the DMX512 protocol
		/// </summary>
		/// <param name="port">Serial port instance</param>
		/// <returns>The referenced serial port instance</returns>
		/// <exception cref="Exception">If the serial port is somehow inaccessible</exception>
		private static SerialPort ConfigureSerialPort(SerialPort port)
		{
			try
			{
				if (port.IsOpen)
					port.Close();

				// Port configuration
				port.BaudRate = 250000;
				port.DataBits = 8;
				port.Handshake = Handshake.None;
				port.Parity = Parity.None;
				port.StopBits = StopBits.Two;

				// Try to open a connection with the given settings
				port.Open();
				port.Close();

				return port;
			}
			catch (Exception exc)
			{
				throw exc;
			}
		}

		/// <summary>
		/// Returns the state of the connection
		/// </summary>
		/// <returns>True if the communication is active</returns>
		public bool IsActive
		{
			get
			{
				lock (this)
				{
					return isActive;
				}
			}
		}

		/// <summary>
		/// Get a parameter value
		/// </summary>
		/// <param name="index">Parameter index between 1 and 512</param>
		/// <returns>Parameter value in bytes</returns>
		/// <exception cref="IndexOutOfRangeException">If the index is not between 1 and 512</exception>
		public byte GetByte(int index)
		{
			if (index < 1 || index > 512)
				throw new IndexOutOfRangeException("Index is not between 1 and 512");

			lock (this)
			{
				return buffer[index];
			}
		}

		/// <summary>
		/// Get all the parameter values
		/// </summary>
		/// <returns>The entire 513 vector</returns>
		public byte[] GetBytes()
		{
			lock (this)
			{
				return buffer;
			}
		}

		/// <summary>
		/// List all DMX-compatible serial ports
		/// </summary>
		/// <returns>A list of all valid serial ports</returns>
		public static List<string> GetValidSerialPorts()
		{
			string[] ports = SerialPort.GetPortNames();
			List<string> portNames = new List<string>();
			foreach (string port in ports)
			{
				try
				{
					ConfigureSerialPort(new SerialPort(port));
					portNames.Add(port);
				}
				catch (Exception) { }
			}
			return portNames;
		}

		/// <summary>
		/// Send the parameters to all slaves in this DMX512 universe
		/// </summary>
		private void SendBytes()
		{
			while (isActive)
			{
				// Send a "zero" for 1ms (must send it for at least 100us)
				serialPort.BreakState = true;
				Thread.Sleep(1);
				serialPort.BreakState = false;
				// Send all the byte parameters
				serialPort.Write(buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		/// Update a parameter value
		/// </summary>
		/// <param name="index">Parameter index between 1 and 512</param>
		/// <param name="value">Parameter value</param>
		/// <exception cref="IndexOutOfRangeException">If the index is not between 1 and 512</exception>
		public void SetByte(int index, byte value)
		{
			if (index < 1 || index > 512)
				throw new IndexOutOfRangeException("Index is not between 1 and 512");

			lock (this)
			{
				buffer[index] = value;
			}
		}

		/// <summary>
		/// Update all parameter values
		/// </summary>
		/// <param name="bytes">A 513 element vector with the first one being a zero</param>
		/// <exception cref="ArgumentException">If the byte vector sent does not contain 513 elements</exception>
		public void SetBytes(byte[] newBuffer)
		{
			if (newBuffer.Length != 513)
				throw new ArgumentException("This byte vector does not contain 513 elements", "newBuffer");


			newBuffer[0] = 0; // Grants that the first byte will be a zero
			lock (this)
			{
				this.buffer = newBuffer;
			}
		}

		/// <summary>
		/// Open the connection and start sending data
		/// </summary>
		public void Start()
		{
			// Prevents it from being started more than once
			if (this.isActive)
				return;

			if (serialPort != null && !serialPort.IsOpen)
				serialPort.Open();
			lock (this)
			{
				this.isActive = true;
			}
			senderThread = new Thread(this.SendBytes);
			senderThread.Start();
		}

		/// <summary>
		/// Stop sending data and close the connection
		/// </summary>
		public void Stop()
		{
			// Prevents it from being stopped more than once
			if (!this.isActive)
				return;

			lock (this)
			{
				this.isActive = false;
			}
			senderThread.Join(1000);
			if (serialPort != null && serialPort.IsOpen)
				serialPort.Close();
		}
	}
}
