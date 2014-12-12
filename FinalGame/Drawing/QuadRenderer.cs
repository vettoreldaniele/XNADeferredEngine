using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysxEngine;

namespace FinalGame
{
    public class QuadRenderer
    {
        // The vertices of the quad (already in projection space).
        private VertexPositionTexCoordRayIndex[] quadVertices;

        // Index buffer for the quad.
        private short[] indexBuffer = null;

        // Reference to the game class.
        Game game;

        public QuadRenderer(Game game)
        {
            this.game = game;

            // Initialize vertices in projection space.
            quadVertices = new VertexPositionTexCoordRayIndex[]
            {
                new VertexPositionTexCoordRayIndex(new Vector3(0, 0, 0), new Vector3(1, 1, 0)),
                new VertexPositionTexCoordRayIndex(new Vector3(0, 0, 0), new Vector3(0, 1, 1)),
                new VertexPositionTexCoordRayIndex(new Vector3(0, 0, 0), new Vector3(0, 0, 3)),
                new VertexPositionTexCoordRayIndex(new Vector3(0, 0, 0), new Vector3(1, 0, 2))
        };

            // Set up the index buffer.
            indexBuffer = new short[] { 0, 1, 2, 2, 3, 0 };
        }

        public void RenderFullScreenQuad(Effect effect)
        {          
            RenderQuad(Vector2.One * -1, Vector2.One, effect);
        }

        public void RenderQuad(Vector2 v1, Vector2 v2, Effect effect)
        {
            Game1 game1 = (Game1)game;

            effect.CurrentTechnique.Passes[0].Apply();

        /*    quadVertices[0].Position.X = v2.X;
            quadVertices[0].Position.Y = v1.Y;

            quadVertices[1].Position.X = v1.X;
            quadVertices[1].Position.Y = v1.Y;

            quadVertices[2].Position.X = v1.X;
            quadVertices[2].Position.Y = v2.Y;

            quadVertices[3].Position.X = v2.X;
            quadVertices[3].Position.Y = v2.Y; */

            quadVertices[0].Position = new Vector3(v2.X, v1.Y, 0);
            quadVertices[1].Position = new Vector3(v1.X, v1.Y, 0);
            quadVertices[2].Position = new Vector3(v1.X, v2.Y, 0);
            quadVertices[3].Position = new Vector3(v2.X, v2.Y, 0);


            game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexCoordRayIndex>
                (PrimitiveType.TriangleList, quadVertices, 0, 4, indexBuffer, 0, 2);


        }

        

        private struct VertexPositionTexCoordRayIndex : IVertexType
        {
            Vector3 vertexPosition;
            Vector3 vertexTextureCoordinate;


            public Vector3 Position
            {
                get { return vertexPosition; }
                set { vertexPosition = value; }
            }

            public Vector3 TextureCoordinate
            {
                get { return vertexTextureCoordinate; }
                set { vertexTextureCoordinate = value; }
            }

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
(
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
);

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return VertexDeclaration; }
            }

            public VertexPositionTexCoordRayIndex(Vector3 position, Vector3 texcoordRayindex)
            {
                vertexPosition = position;
                vertexTextureCoordinate = texcoordRayindex;
            }
        }

    }
}
