using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ReactionTimer
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont Consolas;
		Texture2D Dot;
		Sample sample;
		SoundEffect Beep;
		StateTracker tracker;

		public Game1()
		{
			graphics = new GraphicsDeviceManager( this );
			Content.RootDirectory = "Content";
			tracker = new StateTracker();
		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch( GraphicsDevice );
			Consolas = Content.Load<SpriteFont>( "Consolas" );
			Dot = Content.Load<Texture2D>( "Dot" );
			Beep = Content.Load<SoundEffect>( "beep" );

			sample = new Sample( 1, 0.1 );

		}

		protected override void Update( GameTime gameTime )
		{
			// Allows the game to exit
			if( GamePad.GetState( PlayerIndex.One ).Buttons.Back == ButtonState.Pressed )
				this.Exit();

			sample.Update();

			var timertrigger = TimeSpan.FromMilliseconds( 250 );
			var rest = TimeSpan.FromSeconds( 3 );

			var pivot = 10;
			var c = sample.Current;

			if( c > pivot && tracker.State == State.Low || c <= pivot && tracker.State == State.High )
				tracker.Toggle();

			if( tracker.State == State.High && DateTime.Now.Ticks > tracker.Mark.Ticks + timertrigger.Ticks )
			{
				tracker.Next();
			}

			if( tracker.State == State.Counting && DateTime.Now.Ticks > tracker.Mark.Ticks + tracker.CountR.Ticks )
			{
				Beep.Play();
				tracker.Next();
			}

			if( tracker.State == State.Noise && DateTime.Now.Ticks > tracker.Mark.Ticks + rest.Ticks )
			{
				tracker.Next();
			}

			base.Update( gameTime );
		}

		protected override void Draw( GameTime gameTime )
		{
			var data = sample.Current;
			var c = Color.Black;
			if( tracker.State == State.Counting )
				c = Color.Gray;
			if( tracker.State == State.Noise )
				c = Color.White;
			GraphicsDevice.Clear( c );

			spriteBatch.Begin();

			spriteBatch.DrawString( Consolas, data.ToString(), new Vector2( 80, 100 ), Color.Red );
			spriteBatch.DrawString( Consolas, new TimeSpan( sample.Load ).ToString(), new Vector2( 80, 60 ), Color.Red );
			spriteBatch.DrawString( Consolas, tracker.ToString(), new Vector2( 80, 120 ), Color.White );
			var height = 300;
			spriteBatch.Draw( Dot, new Rectangle( 10, 10, 30, height ), Color.White );
			spriteBatch.Draw( Dot, new Rectangle( 12, 12, 26, (int) ( data * 3 ) ), Color.Green );

			spriteBatch.End();


			base.Draw( gameTime );
		}
	}
}
