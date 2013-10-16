using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ReactionTimer
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager _graphics;
		SpriteBatch _spriteBatch;
		SpriteFont _consolas;
		Texture2D _dot;
		Sample _sample;
		SoundEffect _beep;
		readonly StateTracker _tracker;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager( this );
			Content.RootDirectory = "Content";
			_tracker = new StateTracker();
		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch( GraphicsDevice );
			_consolas = Content.Load<SpriteFont>( "Consolas" );
			_dot = Content.Load<Texture2D>( "Dot" );
			_beep = Content.Load<SoundEffect>( "beep" );

			_sample = new Sample( 1, 0.1 );

		}

		protected override void Update( GameTime gameTime )
		{
			// Allows the application to exit
			if( GamePad.GetState( PlayerIndex.One ).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown( Keys.Escape ) )
				Exit();

			//record from mic
			_sample.Update();

			var timertrigger = TimeSpan.FromMilliseconds( 250 );
			var rest = TimeSpan.FromSeconds( 3 );

			var pivot = 10;
			var c = _sample.AudioRMS;

			if( c > pivot && _tracker.CurrentState == State.Low || c <= pivot && _tracker.CurrentState == State.High )
				_tracker.Toggle();

			if( _tracker.CurrentState == State.High && DateTime.Now.Ticks > _tracker.Mark + timertrigger.Ticks )
			{
				_tracker.Next();
			}

			if( _tracker.CurrentState == State.Counting && DateTime.Now.Ticks > _tracker.Mark + _tracker.CountingInstanceDuration.Ticks )
			{
				_beep.Play();
				_tracker.Next();
			}

			if( _tracker.CurrentState == State.Noise && DateTime.Now.Ticks > _tracker.Mark + rest.Ticks )
			{
				_tracker.Next();
			}

			base.Update( gameTime );
		}
		int _max;
		protected override void Draw( GameTime gameTime )
		{
			var data = _sample.AudioRMS;
			var c = Color.Black;
			if( _tracker.CurrentState == State.Counting )
				c = Color.DarkBlue;
			if( _tracker.CurrentState == State.Noise )
				c = Color.Gray;
			GraphicsDevice.Clear( c );
			int load = (int) ( data * 3 );
			_max = Math.Max( _max, load );
			_spriteBatch.Begin();

			_spriteBatch.DrawString( _consolas, data.ToString(), new Vector2( 80, 100 ), Color.Red );
			_spriteBatch.DrawString( _consolas, new TimeSpan( _sample.Load ).ToString(), new Vector2( 80, 60 ), Color.Red );
			_spriteBatch.DrawString( _consolas, _tracker.ToString(), new Vector2( 80, 120 ), Color.White );
			_spriteBatch.Draw( _dot, new Rectangle( 10, 10, 30, _max ), Color.White );
			_spriteBatch.Draw( _dot, new Rectangle( 12, 12, 26, load ), Color.Green );

			_spriteBatch.End();


			base.Draw( gameTime );
		}
	}
}
