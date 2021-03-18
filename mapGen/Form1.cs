using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mapGen.Properties;
using SharpGL;

namespace mapGen
{
    public partial class Form1 : Form
    {

        private OpenGL gl;

        private float offX = 0;//вращение камеры
        private float offY = 100;//высота камеры
        private float cameraDist = 200;//дистация камеры от центра
        private int maxHeightMap = 50;//максимальная высота гор с 100% яркостью
        private float sensivity = 5;//Чувстительность мыши

        //для вращения
        private int lastX = 0;
        private int lastY = 0;
        private bool isDrag = false;
        
        
        int xC = 0;//Количество точек по х
        int zC = 0;//Количество точек по y
        int[,] points ;//массив высот, 

        private void openGLControl1_MouseUp(object sender, MouseEventArgs e){
            if (e.Button == MouseButtons.Left)
                isDrag = false;
        }
        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                offX += (e.X - lastX) / sensivity;
                offY += (lastY - e.Y) / sensivity;
            }
            lastX = e.X;
            lastY = e.Y;
        }
        private void openGLControl1_MouseDown(object sender, MouseEventArgs e){
            if (e.Button == MouseButtons.Left){
                lastX = e.X;
                lastY = e.Y;
                isDrag = true;
            }
        }

        public Form1(){
            InitializeComponent();
            gl = this.openGLControl1.OpenGL;
            openGLControl1.MouseWheel += openGLControl1_MouseWheel;
            gl.ClearColor(1, 1, 1, 0);
            
        }
        void openGLControl1_MouseWheel(object sender, MouseEventArgs e){
            if (e.Delta < 0 && cameraDist < 500)
                cameraDist += 20;
            if (e.Delta > 0 && cameraDist > 20)
                cameraDist -= 20;
        }
        private void Form1_Load(object sender, EventArgs e){

            var bmp = new Bitmap(Properties.Resources.noise);//Берем изображение шума из ресурсов
            xC = bmp.Width;
            zC = bmp.Height;
            points = new int[xC, zC];
            for (int i = 0; i < xC; i++)
                for (int j = 0; j < zC; j++){
                    points[i, j] = (int)(bmp.GetPixel(i, j).GetBrightness() * maxHeightMap);//Генерируем карту высот по яркости пикселем
                }
        }
        public void setColor(int val, OpenGL gl){
            double col = (double)val/(double)maxHeightMap * 0.8f;//цвет вершин чем выше тем светлее 
            gl.Color(col, col, col);
        }
        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args){
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            var width = openGLControl1.Width;
            var height = openGLControl1.Height;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 500.0);

            //Вращение камеры
            double angleT = (offX%360)*Math.PI/180f;
            double vx = xC / 2 + Math.Cos(angleT) * cameraDist;
            double vz = zC / 2 + Math.Sin(angleT) * cameraDist;
            gl.LookAt(vx, offY%200, vz, xC / 2, maxHeightMap / 2, zC / 2 , 0, 50, 0);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            double color = 0;
            //Рисуем карту по треугольникам
            for (int x = 0; x < xC-1; x++)
                for (int z = 0; z < zC-1; z++){
                    gl.Begin(OpenGL.GL_TRIANGLES);

                    setColor(points[x, z], gl);
                    gl.Vertex(x,  points[x, z],z);
                    setColor(points[x+1, z], gl);
                    gl.Vertex(x + 1, points[x+1, z],z);
                    setColor(points[x, z+1], gl);
                    gl.Vertex(x, points[x, z + 1], z + 1);
                    gl.End();

                    gl.Begin(OpenGL.GL_TRIANGLES);
                    setColor(points[x+1, z], gl);
                    gl.Vertex(x + 1,  points[x + 1, z],z);
                    setColor(points[x + 1, z+1], gl);
                    gl.Vertex(x + 1, points[x + 1, z + 1], z + 1);
                    setColor(points[x , z+1], gl);
                    gl.Vertex(x,  points[x, z + 1],z + 1);
                    gl.End();
                }
        }



    }
}
