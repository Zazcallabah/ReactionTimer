using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ReactionTimer
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
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

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch( GraphicsDevice );
			Consolas = Content.Load<SpriteFont>( "Consolas" );
			Dot = Content.Load<Texture2D>( "Dot" );
			Beep = Content.Load<SoundEffect>( "beep" );

			sample = new Sample( 1, 0.1 );

		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update( GameTime gameTime )
		{
			// Allows the game to exit
			if( GamePad.GetState( PlayerIndex.One ).Buttons.Back == ButtonState.Pressed )
				this.Exit();

			sample.Update();

			var pivot = 10;
			var c = sample.Current;

			if( c > pivot && tracker.State == State.Low || c <= pivot && tracker.State == State.High )
				tracker.Toggle();


			base.Update( gameTime );
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime )
		{
			var data = sample.Current;
			GraphicsDevice.Clear( Color.Black );

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
