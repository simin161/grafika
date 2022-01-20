// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using SharpGL.SceneGraph.Cameras;
using SharpGL.Enumerations;
using System.Drawing.Imaging;
using System.Drawing;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        private enum TextureObjects { Ice = 0, Water, Snow };
        private readonly int m_textureCount =3;

        private uint[] m_textures = null;
        private string[] m_textureFiles = { "..//..//images//ice.jpg", "..//..//images//water1.jpg", "..//..//images//snow.jpg" };

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 10f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private LookAtCamera lookAtCam;

        private Vertex direction;
        private Vertex right;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
           // float[] whiteLight = { 0.0f, 0.0f, 1.0f, 1.0f };

            // Model sencenja na flat (konstantno)
            lookAtCam = new LookAtCamera();
            lookAtCam.Position = new Vertex(0f, 0f, 5f);
            lookAtCam.Target = new Vertex(0f, 0f, -100f);
            lookAtCam.UpVector = new Vertex(0f, 100f, 0f);
            right = new Vertex(1f, 0f, 0f);
            direction = new Vertex(0.75f, 0f, -5f);
            lookAtCam.Target = lookAtCam.Position + direction;
            lookAtCam.Project(gl);
            
            //gl.ShadeModel(OpenGL.GL_FLAT);

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            //blending; stapanje
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            // Ucitaj slike i kreiraj teksture
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);      // Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                image.UnlockBits(imageData);
                image.Dispose();
            }
            //gl.ShadeModel(OpenGL.GL_FLAT);
            gl.ClearColor(0f, 0f, 0f, 1.0f);
            SetupLighting(gl);
            m_scene.LoadScene();
            m_scene.Initialize();
        }

        private void SetupLighting(OpenGL gl)
        {
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
           // gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_EXPONENT, 5.0f);
           // float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
           // gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);

            float[] light0pos = new float[] { 60.0f, 30.0f, -30.0f, 1f };
            float[] light0ambient = new float[] { 0.4f, 0.4f, 0.4f, 1f };
            float[] light0diffuse = new float[] { 1f, 1f, 0.2f, 1f };
            //float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 40.0f);
           // gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_EXPONENT, 50.0f);
            //float[] global_ambient1 = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient1);

            float[] light1pos = new float[] { 20.0f, 30.0f, -20.0f, 1.0f };
            float[] light1ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light1diffuse = new float[] { 1f, 0f, 0f, 1.0f };
          //  float[] light1specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };
            float[] direction = new float[] { 20.0f, -30.0f, -20.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
          //  gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, direction);
            //gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT1);

            // Ukljuci automatsku normalizaciju nad normalama
            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();

            gl.Perspective(50.0, (double)m_width / m_height, 0.5f, 50000f);
            gl.Viewport(0, 0, m_width, m_height);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            lookAtCam.Project(gl);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();

            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.PushMatrix();
            gl.Scale(10, 10, 10);
            gl.Translate(0f, -0.5f, 0f);
            //gl.Color(1f, 1f, 1f);
                m_scene.Draw();
            gl.PopMatrix();

            DrawIce(gl);
            DrawWater(gl);
            DrawIgloo(gl);
            //DrawLight(gl);
            DrawText(gl);
            gl.PopMatrix();

              // Oznaci kraj iscrtavanja
              gl.Flush();
          }

          public void DrawLight(OpenGL gl)
          {
            gl.PushMatrix();
          //    gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_REPLACE);
              gl.Translate(20f, 30f, -30f);
              Sphere sphereLamp = new Sphere();
              sphereLamp.CreateInContext(gl);
              sphereLamp.Radius = 3f;
              sphereLamp.Material = new SharpGL.SceneGraph.Assets.Material();
              sphereLamp.Material.Emission = Color.Red;
              sphereLamp.Material.Bind(gl);
              sphereLamp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
        }
        public void DrawIce(OpenGL gl)
        {
            gl.MatrixMode(MatrixMode.Texture);

            gl.PushMatrix();
            //gl.Color(0f, 0f, 0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Ice]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f);
            gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(0.0f, 0.0f);
                    gl.Vertex(40, -5, -50);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(1.0f, 0.0f);
                    gl.Vertex(-40, -5, -50);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(1.0f, 1.0f);
                    gl.Vertex(-40, -5, 20);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(0.0f, 1.0f);
                    gl.Vertex(40, -5, 20);
                    gl.Color(0f, 0f, 0f);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(0.0f, 0.0f);
                    gl.Vertex(40, -5, 20);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(1.0f, 0.0f);
                    gl.Vertex(-40, -5, 20);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(1.0f, 1.0f);
                    gl.Vertex(-40, -6.5, 20);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(0.0f, 1.0f);
                    gl.Vertex(40, -6.5, 20);
                gl.End();
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);

        }

        public void DrawWater(OpenGL gl)
        {
            gl.MatrixMode(MatrixMode.Texture);

            gl.PushMatrix();
             //   gl.Color(0f,0f, 0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Water]);
                gl.Begin(OpenGL.GL_QUADS);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(0.0f, 0.0f);
                    gl.Vertex(40, -6.5, 20);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(1.0f, 0.0f);
                    gl.Vertex(-40, -6.5, 20);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(1.0f, 1.0f);
                    gl.Vertex(-40, -6.5, 50);
                    gl.Normal(0f, 1f, 0f);
                    gl.TexCoord(0.0f, 1.0f);
                    gl.Vertex(40, -6.5, 50);
                gl.End();
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);

        }

        public void DrawIgloo(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Color(1f,1f,1f);
            gl.Translate(20f, -2f, -20f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_T);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Snow]);
            //gl.TexGen(OpenGL.GL_S, OpenGL.GL_OBJECT_PLANE, OpenGL.GL_SPHERE_MAP);
            gl.Scale(12f, 12f, 12f); 
                Sphere sp = new Sphere();
                sp.CreateInContext(gl);
                sp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                gl.Color(1f, 1f, 1f);
                gl.Translate(-1.3f, -0.2f, 0f);
                gl.Scale(2f, 0.6f, 0.5f);
                gl.Rotate(0f, 90f, 0f);
                Cylinder cy = new Cylinder();
                cy.TopRadius = 1;
                cy.CreateInContext(gl);
                cy.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_T);
        }


        public void DrawText(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Viewport(m_width - 255, 0, m_width/2, m_height/2);
            gl.LoadIdentity();
            gl.DrawText(5, 75, 1.0f, 0.0f, 0.0f, "Courier New", 10.0f, " ");
            gl.DrawText(0, 100, 1.0f, 0.0f, 0.0f, "Verdana italic", 10.0f, "Predmet: Racunarksa grafika");
            gl.DrawText(0, 80, 1.0f, 0.0f, 0.0f, "Verdana italic", 10.0f, "Sk.god: 2021/22.");
            gl.DrawText(0, 60, 1.0f, 0.0f, 0.0f, "Verdana italic", 10.0f, "Ime: Natalija");
            gl.DrawText(0, 40, 1.0f, 0.0f, 0.0f, "Verdana italic", 10.0f, "Prezime: Simin");
            gl.DrawText(0, 20, 1.0f, 0.0f, 0.0f, "Verdana italic", 10.0f, "Sifra zad: 14.2");
            gl.PopMatrix();
        }
        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50.0, (double)m_width / m_height, 0.5f, 50000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
