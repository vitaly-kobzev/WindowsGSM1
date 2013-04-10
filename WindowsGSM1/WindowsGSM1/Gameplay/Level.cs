using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
    public class Level
    {
        private ContentManager _content;

	    private Engine _engine;

		public Vector2 StartLocation { get; private set; }

		public Point ExitPosition { get; private set; }

        // Physical structure of the level.
        private Tile[,] _tiles;

        // Level game state.
        private Random _random = new Random(354668); // Arbitrary, but constant seed

        private static readonly Point InvalidPosition = new Point(-1, -1);

        public Level(ContentManager content, Engine engine)
        {
            _content = content;

	        _engine = engine;

	        ExitPosition = InvalidPosition;
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return _tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return _tiles.GetLength(1); }
        }

		public float GroundLevel { get{return Height*Tile.Height;} }

	    /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y, out GameObject collidedTile)
        {
	        collidedTile = null;
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

	        var tile = _tiles[x, y];
	        collidedTile = tile;

	        return tile == null ? TileCollision.Passable : tile.Collision;
        }

		public void RemoveTileAt(int x, int y)
		{
			_tiles[x, y] = null;
		}

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        #region Loading

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public GameObject[] LoadLevel(Stream fileStream)
        {
			var loadedObjects = new List<GameObject>();

            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            _tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char objType = lines[y][x];
                    var obj = LoadObject(objType, x, y);
					if(obj is Tile)
						_tiles[x, y] = (Tile)obj;

					if (obj != null)
						loadedObjects.Add(obj);
                }
            }

             //Verify that the level has a beginning and an end.
            if (StartLocation == null)
				throw new NotSupportedException("A level must have a starting point.");
            if (ExitPosition == InvalidPosition)
				throw new NotSupportedException("A level must have an exit.");

	        return loadedObjects.ToArray();
        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private GameObject LoadObject(char tileType, int x, int y)
        {
	        var position = new Vector2(x, y)*Tile.Size;
            switch (tileType)
            {
                // Blank space
                case '.':
		            return null;

                // Exit
                case 'X':
					return LoadExitTile(x,y);

                // Ground
                case '-':
					return LoadTile("Platform", position, TileCollision.Impassable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Killable tile
                case '#':
					return LoadKillableTile("BlockA6", position, TileCollision.Impassable);

				// Killable tile
				case 'Y':
					return LoadKillableObject("Exit", position, CollisionCheckType.PerPixel);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

	    private DestructableObject LoadKillableObject(string name, Vector2 position, CollisionCheckType perPixel)
	    {
			return new DestructableObject("Tiles/" + name, position, perPixel, _engine);
	    }

	    private Tile LoadKillableTile(string name, Vector2 position, TileCollision collision)
        {
            return new DestructableTile(3,"Tiles/" + name, position, collision, _engine);
        }

        private Tile LoadTile(string name,Vector2 position, TileCollision collision)
        {
            return new GroundTile("Tiles/" + name, position, collision, _engine);
        }


        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x,int y)
        {
            if (ExitPosition != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

			ExitPosition = GetBounds(x, y).Center;

            return LoadTile("Exit",new Vector2(x, y)*Tile.Size, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            StartLocation = RectangleExtensions.GetBottomCenter(GetBounds(x, y));

			return LoadTile("Exit", new Vector2(x, y) * Tile.Size, TileCollision.Passable);
        }
        #endregion

    }
}
