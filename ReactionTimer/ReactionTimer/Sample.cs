using System;
using Microsoft.Xna.Framework.Audio;

namespace ReactionTimer
{
	public class Sample
	{
		/// <summary>
		/// Sets buffer length to 10 times the interval.
		/// </summary>
		/// <param name="interval">RMS calculation interval in seconds.</param>
		public Sample( double interval ) : this( interval * 10, interval ) { }

		/// <summary>
		/// Buffer length needs to be large enough so that all audio recorded between frames can fit in it.
		/// For 24 fps this means 1/24th of a second, so 1 second is probably ok?
		/// 
		/// Calculation interval should be large enough to be responsive, and small enough to provide adequate data.
		/// </summary>
		/// <param name="bufferLength">Buffer length in seconds.</param>
		/// <param name="calculationInterval">RMS calculation interval in seconds.</param>
		public Sample( double bufferLength, double calculationInterval )
		{
			if( bufferLength < calculationInterval )
				throw new ArgumentException( "It is probably best if the length of the buffer is larger than the size of the calculation interval", "bufferLength" );

			_intervalLengthInTicks = TimeSpan.FromSeconds( calculationInterval ).Ticks;
			_audioDataRingBuffer = new byte[Microphone.Default.GetSampleSizeInBytes( TimeSpan.FromSeconds( bufferLength ) )];
			_intervalMark = DateTime.Now.Ticks;
			Microphone.Default.Start();
		}

		/// <summary>
		/// Load is the number of ticks spend on calculating RMS every interval.
		/// </summary>
		public long Load { get; private set; }

		/// <summary>
		/// This number describes the percieved loudness of the sound recorded from the default mic during the last interval.
		/// </summary>
		public double AudioRMS { get; private set; }

		/// <summary>
		/// This is a ring buffer. That just means that any indexing done into it has to be done modulo buffer length.
		/// No guards are in place to prevent the buffer from eating its own tail.
		/// </summary>
		readonly byte[] _audioDataRingBuffer;

		/// <summary>
		/// This is where audio will next be written into the buffer.
		/// </summary>
		long _nextBufferPositionToFill;

		/// <summary>
		/// All buffer positions from 0 to this (non inclusive upper bound) has already been through a rms calculation, and can be discarded.
		/// </summary>
		long _bufferPositionAlreadyCalculated;


		readonly long _intervalLengthInTicks;
		long _intervalMark;

		/// <summary>
		/// Called once for frame
		/// </summary>
		public void Update()
		{
			if( DateTime.Now.Ticks > _intervalMark + _intervalLengthInTicks )
			{
				RecalculateRMS();
				_intervalMark = DateTime.Now.Ticks;
			}

			// fill buffer with microphone data collected since last frame
			_nextBufferPositionToFill += Microphone.Default.GetData(
				_audioDataRingBuffer,
				(int) ( _nextBufferPositionToFill % _audioDataRingBuffer.Length ),
				( _audioDataRingBuffer.Length - (int) ( _nextBufferPositionToFill % _audioDataRingBuffer.Length ) ) );
		}

		void RecalculateRMS()
		{
			if( _bufferPositionAlreadyCalculated >= _nextBufferPositionToFill )
			{
				Load = 0;
				return;
			}

			long mark = DateTime.Now.Ticks;

			var shorts = ConvertRingBufferToShortArray( _bufferPositionAlreadyCalculated, _nextBufferPositionToFill - _bufferPositionAlreadyCalculated );

			// calculate the mean value of the samples
			long sum = 0;
			for( long i = 0; i < shorts.Length; i++ )
			{
				sum += shorts[i];
			}
			long mean = ( sum / shorts.Length );

			// sum the squares of the delta between mean and sample
			long sqsum = 0;
			for( long i = 0; i < shorts.Length; i++ )
			{
				var d = ( shorts[i] - mean );
				sqsum += d * d;
			}

			AudioRMS = ( Math.Sqrt( sqsum ) / ( shorts.Length ) );
			Load = DateTime.Now.Ticks - mark;
			_bufferPositionAlreadyCalculated = _nextBufferPositionToFill;
		}

		/// <summary>
		/// Audio data is unsigned short little-endian PCM.
		/// Thus we need to convert the appropriate part of the ring buffer from a byte array to a short array.
		/// </summary>
		/// <param name="offset">Ring buffer index where extration starts</param>
		/// <param name="count">Number of bytes to extract</param>
		short[] ConvertRingBufferToShortArray( long offset, long count )
		{
			if( count % 2 != 0 )
				throw new ArgumentException( "Number of collected bytes need to be even to be able to form shorts", "count" );

			var convertedResult = new short[count / 2];
			for(
				long ringbufferIndex = offset, result_index = 0;
				ringbufferIndex < offset + count;
				ringbufferIndex += 2, result_index++ )
			{
				convertedResult[result_index]
					= (short) ( _audioDataRingBuffer[ringbufferIndex % _audioDataRingBuffer.Length]
					| ( ( _audioDataRingBuffer[( ringbufferIndex + 1 ) % _audioDataRingBuffer.Length] ) << 8 ) );
			}
			return convertedResult;
		}
	}
}
