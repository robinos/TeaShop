using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Globalization;

namespace TestTeaShop
{
	public class Clock
	{
		public EventHandler lastCall;
		public EventHandler startServing;
		public EventHandler stopServing;
		public EventHandler closingTime;
		private static Timer myClockTimer;
		private static CultureInfo cultureInfo = new CultureInfo("sv-SE");
		private static DateTime currentTime;
		private static DateTime startTime;
		private static DateTime lastCallTime;
		private static DateTime stopServingTime;
		private static DateTime endTime;
		private const string timeFormat = "HH:mm:ss";
		private bool pastServing = false;
		private bool pastLastCall = false;
		private bool pastStopServing = false;
		private bool pastClose = false;
		private Owner owner;

		public DateTime CurrentTime
		{
			get { return currentTime; }
		}

		public Clock(Owner owner)
		{
			DateTime.TryParse("14:55:00", cultureInfo, DateTimeStyles.None, out currentTime);
			DateTime.TryParse("15:00:00", cultureInfo, DateTimeStyles.None, out startTime);
			DateTime.TryParse("15:30:00", cultureInfo, DateTimeStyles.None, out lastCallTime);
			DateTime.TryParse("15:45:00", cultureInfo, DateTimeStyles.None, out stopServingTime);
			DateTime.TryParse("16:00:00", cultureInfo, DateTimeStyles.None, out endTime);
			myClockTimer = new Timer(1000); //1 seconds
			myClockTimer.Elapsed += OnTimedEvent;
			myClockTimer.Enabled = true;
			Console.WriteLine("\nCurrent time: " + ReportTime() + "\n");
			this.owner = owner;
		}

		public string ReportTime()
		{
			return(currentTime.ToString(timeFormat));
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			Clock.currentTime = currentTime.AddMinutes(1);
			Console.WriteLine("\nCurrent time: " + ReportTime() + "\n");

			if (currentTime.CompareTo(endTime) >= 0 && !pastClose)
			{
				Console.WriteLine("\n**Closing time: " + ReportTime() + "**\n");
				SendClosingTime();
				pastClose = true;
			}
			else if (currentTime.CompareTo(stopServingTime) >= 0 && !pastStopServing)
			{
				Console.WriteLine("\n**Stop serving: " + ReportTime() + "**\n");
				SendStopServing();
				pastStopServing = true;
			}
			else if (currentTime.CompareTo(lastCallTime) >= 0 && !pastLastCall)
			{
				Console.WriteLine("\n**Last call: " + ReportTime() + "**\n");
				SendLastCall();
				pastLastCall = true;
			}
			else if (currentTime.CompareTo(startTime) >= 0 && !pastServing)
			{
				Console.WriteLine("\n**Opening time: " + ReportTime() + "**\n");
				SendStartServing();
				pastServing = true;
			}
		}

		private async Task SendStartServing()
		{
			//startServing(this, new EventArgs());
			await owner.ReceiveAndStartServing();
		}

		private async Task SendLastCall()
		{
			//lastCall(this, new EventArgs());
			await owner.ReceiveAndSendLastCall();
		}

		private async Task SendStopServing()
		{
			//stopServing(this, new EventArgs());
			await owner.ReceiveAndStopServing();
		}

		private async Task SendClosingTime()
		{
			//closingTime(this, new EventArgs());
			await owner.ReceiveClosingTime();
		}

		public void receivedShutDown()
		{
			stopClock();
		}

		private void stopClock()
		{
			myClockTimer.Stop();
		}
	}
}
