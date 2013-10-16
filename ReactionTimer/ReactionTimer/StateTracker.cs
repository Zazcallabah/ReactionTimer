using System;

namespace ReactionTimer
{
	/// <summary>
	/// Keeps track of what state the application is in and for how long it has been in it.
	/// Also provides methods for moving between states.
	/// </summary>
	class StateTracker
	{
		readonly Random _r;

		State _currentState;

		public State CurrentState
		{
			get { return _currentState; }
			private set
			{
				Mark = DateTime.Now.Ticks;
				_currentState = value;
			}
		}

		/// <summary>
		/// The last time we changed state.
		/// </summary>
		public long Mark { get; private set; }

		/// <summary>
		/// This is ugly. Keep track of the random duration we should spend in the counting state.
		/// </summary>
		public TimeSpan CountingInstanceDuration { get; private set; }

		public StateTracker()
		{
			_r = new Random();
			Mark = DateTime.Now.Ticks;
		}

		/// <summary>
		/// Toggle between high and low state.
		/// </summary>
		public void Toggle()
		{
			if( CurrentState == State.Low )
				CurrentState = State.High;
			else if( CurrentState == State.High )
				CurrentState = State.Low;
		}

		/// <summary>
		/// Proceed to the next state.
		/// </summary>
		public void Next()
		{
			switch( CurrentState )
			{
				case State.Low:
				case State.High:
					CurrentState = State.Counting;
					CountingInstanceDuration = TimeSpan.FromSeconds( _r.NextDouble() * 3 + 1 );
					break;
				case State.Counting:
					CurrentState = State.Noise;
					break;
				default:
					CurrentState = State.Low;
					break;
			}
		}

		public override string ToString()
		{
			return CurrentState + " " + new TimeSpan( DateTime.Now.Ticks - Mark ).ToString();
		}
	}
}