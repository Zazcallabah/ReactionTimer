using System;
using Microsoft.Xna.Framework.Audio;

namespace ReactionTimer
{
	public class Sample
	{
		public long Load { get; private set; }
		public double Current
		{
			get;
			private set;
		}
		readonly byte[] _buffer;
		long _bytesRead;
		long _calculatedbytes;
		public long IntervalMark { get; private set; }
		public long Interval { get; private set; }

		public long StartMark { get; private set; }
		public long Duration { get; private set; }
		public Sample( double length, double interval )
		{
			var ts = TimeSpan.FromSeconds( length );
			var iv = TimeSpan.FromSeconds( interval );
			Interval = iv.Ticks;
			Duration = ts.Ticks;
			_buffer = new byte[Microphone.Default.GetSampleSizeInBytes( new TimeSpan( 0, 0, 15 ) )];
			IntervalMark = DateTime.Now.Ticks;
			ResetBuffer();
			Microphone.Default.Start();
		}

		void Calculate()
		{
			if( _calculatedbytes >= _bytesRead )
				return;
			long mark = DateTime.Now.Ticks;


			var shorts = ConvertToShort( _calculatedbytes, _bytesRead - _calculatedbytes );
			long sum = 0;
			for( long i = 0; i < shorts.Length; i++ )
			{
				sum += shorts[i];
			}

			long mean = ( sum / shorts.Length );

			long sqsum = 0;
			for( long i = 0; i < shorts.Length; i++ )
			{
				var d = ( shorts[i] - mean );
				sqsum += d * d;
			}

			IntervalMark = DateTime.Now.Ticks;
			Current = ( Math.Sqrt( sqsum ) / ( shorts.Length ) );
			_calculatedbytes = _bytesRead;
			Load = DateTime.Now.Ticks - mark;
		}
		short[] ConvertToShort( long offset, long count )
		{
			if( count % 2 != 0 )
				throw new Exception();
			var s = new short[count / 2];
			for( long i = offset, si = 0; i < offset + count; i += 2, si++ )
			{
				s[si] = (short) ( _buffer[i % _buffer.Length] | ( ( _buffer[( i + 1 ) % _buffer.Length] ) << 8 ) );
			}
			return s;
		}

		void ResetBuffer()
		{
			StartMark = DateTime.Now.Ticks;
		}

		public void Update()
		{
			if( DateTime.Now.Ticks > StartMark + Duration )
			{
				ResetBuffer();
			}

			if( DateTime.Now.Ticks > IntervalMark + Interval )
			{
				Calculate();
			}
			_bytesRead += Microphone.Default.GetData(
				_buffer,
				(int) ( _bytesRead % _buffer.Length ),
				( _buffer.Length - (int) ( _bytesRead % _buffer.Length ) ) );
		}

	}
}