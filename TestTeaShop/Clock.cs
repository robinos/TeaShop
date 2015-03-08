using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Globalization;

namespace TestTeaShop
{
	/// <summary>
	/// Clock objektet hanterar tiden och klockslag för viktiga händelser som
	/// början, lastcall, sluta servera, stängdags och slutet.
	/// </summary>
	public class Clock
	{
		//händelser för klockan
		public EventHandler lastCall;
		public EventHandler startServing;
		public EventHandler stopServing;
		public EventHandler closingTime;
		//Klockan
		private static Timer myClockTimer;
		//Kulturinfo och DateTime variabler för tidspunkter
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
		//Owner objektet
		private Owner owner;

		/// <summary>
		/// CurrentTime ger tiden just nu i simuleringen.
		/// </summary>
		public DateTime CurrentTime
		{
			get { return currentTime; }
		}

		/// <summary>
		/// Clock tar emot en owner objekt och startar tidsberäkningen.
		/// </summary>
		/// <param name="owner"></param>
		public Clock(Owner owner)
		{
			DateTime.TryParse("14:55:00", cultureInfo, DateTimeStyles.None, out currentTime);
			DateTime.TryParse("15:00:00", cultureInfo, DateTimeStyles.None, out startTime);
			DateTime.TryParse("15:30:00", cultureInfo, DateTimeStyles.None, out lastCallTime);
			DateTime.TryParse("15:45:00", cultureInfo, DateTimeStyles.None, out stopServingTime);
			DateTime.TryParse("16:00:00", cultureInfo, DateTimeStyles.None, out endTime);
			myClockTimer = new Timer(1000); //1 seconds

			//OnTimedEvent blir observer till myClockTimer.Elapsed (efter varje tick av klockan)
			myClockTimer.Elapsed += OnTimedEvent;
	
			//Startar klockan
			myClockTimer.Enabled = true;
			Console.WriteLine("\nCurrent time: " + ReportTime() + "\n");
			this.owner = owner;
		}

		/// <summary>
		/// En hjälp metoder för att reportera tiden till consolen.
		/// </summary>
		/// <returns>En sträng med tidsinformation</returns>
		public string ReportTime()
		{
			return(currentTime.ToString(timeFormat));
		}

		/// <summary>
		/// OnTimedEvent körs varje tick (sekund) och skickar ut metodanrop när
		/// viktiga händelser händer sker för klockan som 'endTime', 'stopServingTime',
		/// 'lastCallTime', och 'startTime'
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// SendStartServing signalera till Owner objektet att nu ska den börjar
		/// ta emot beställningar genom att anropa RecieveAndStartServing.
		/// </summary>
		/// <returns>Task (tom)</returns>
		private async Task SendStartServing()
		{
			await owner.ReceiveAndStartServing();
		}

		/// <summary>
		/// SendLastCall signalera till Owner objektet att nu är det sista
		/// beställningar (det är kunder som reagera på det). Det görs med
		/// SendLastCall.
		/// </summary>
		/// <returns>Task (tom)</returns>
		private async Task SendLastCall()
		{
			await owner.ReceiveAndSendLastCall();
		}

		/// <summary>
		/// SendStopServing signalera till Owner objektet att nu ska den sluta
		/// ta emot beställningar. Det görs med ReceiveAndStopServing.
		/// </summary>
		/// <returns>Task (tom)</returns>
		private async Task SendStopServing()
		{
			await owner.ReceiveAndStopServing();
		}

		/// <summary>
		/// SendClosingTime signalera till Owner objektet att nu är det stängdags.
		/// Det görs med ReceiveClosingTime.
		/// </summary>
		/// <returns>Task (tom)</returns>
		private async Task SendClosingTime()
		{
			await owner.ReceiveClosingTime();
		}

		/// <summary>
		/// receivedShutDown är en hjälp metod för att stänga ner timer
		/// tråden vid avstängning.
		/// </summary>
		public void receivedShutDown()
		{
			stopClock();
		}

		/// <summary>
		/// stopClock stoppar timer tråden.
		/// </summary>
		private void stopClock()
		{
			myClockTimer.Stop();
		}
	}
}
