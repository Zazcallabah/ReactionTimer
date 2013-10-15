using System;

namespace ReactionTimer
{
	class StateTracker
	{
		public State State { get; private set; }
		public DateTime Mark { get; private set; }

		public StateTracker()
		{
			Mark = DateTime.Now;
		}

		public void Toggle()
		{
			if( State == State.Low )
			{
				State = State.High;
			}
			else if( State == State.High )
			{
				State = State.Low;
			}
			Mark = DateTime.Now;
		}

		public void Next()
		{
			switch( State )
			{
				case State.Low:
				case State.High:
					State = State.Counting;
					break;
				case State.Counting:
					State = State.Noise;
					break;
				case State.Noise:
					State = State.Waiting;
					break;
				default:
					State = State.Low;
					break;
			}
			Mark = DateTime.Now;
		}

		public override string ToString()
		{
			return State + " " + new TimeSpan( DateTime.Now.Ticks - Mark.Ticks ).ToString();
		}
	}
}