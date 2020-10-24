using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Maze_Learner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Maze maze = new Maze(100, 180);
        List<uint> nodes = new List<uint>();
        public MainWindow()
        {
            InitializeComponent();

            //set width and height of the canvas according to screen resolution
            canvasMaze.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 150;
            canvasMaze.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 100;

            //maze = new Maze(5, 5);

            //DisplayMaze();

            //GenerateMaze();
        }

        private void DisplayMaze()
        {
            canvasMaze.Children.Clear();

            double wallSize = (maze.Rows > maze.Columns ? canvasMaze.Height / maze.Rows : (maze.Rows == maze.Columns ? canvasMaze.Height / maze.Rows : canvasMaze.Height / maze.Rows));
            if (maze.Rows <= 10 && maze.Columns <= 10) wallSize = 65;

            double strokeSize = wallSize < 40 ? wallSize / 6.0 : 4;
            double startX = (canvasMaze.Width / 2 - (wallSize * maze.Columns / 2));
            double startX_Copy = startX;
            double startY = (canvasMaze.Height / 2 - (wallSize * maze.Rows / 2));

            for (ushort i = 0; i < maze.Rows; i++)
            {
                for (ushort j = 0; j < maze.Columns; j++)
                {
                    ButtonInfo info = new ButtonInfo();
                    info.Xcoord = startX + wallSize * j;
                    info.Ycoord = startY + wallSize * i;
                    info.CellId = maze.CellID(i, j);
                    info.wallSize = wallSize;
                    info.strokeSize = strokeSize;
                    AddButton(startX + wallSize * j, startY + wallSize * i, wallSize, info);
                }
            }

            for (int i = 0; i < maze.Rows; i++)
            {
                for (int j = 0; j < maze.Columns; j++)
                {
                    //Top
                    if (!maze.Matrix[i, j, 0])
                    {
                        DrawLine(startX, startY, startX + wallSize, startY, strokeSize, Brushes.Black);
                    }
                    //Bottom
                    if (!maze.Matrix[i, j, 2])
                    {
                        DrawLine(startX, startY + wallSize, startX + wallSize, startY + wallSize, strokeSize, Brushes.Black);
                    }
                    //Left
                    if (!maze.Matrix[i, j, 3])
                    {
                        DrawLine(startX, startY, startX, startY + wallSize, strokeSize, Brushes.Black);
                    }
                    //Right
                    if (!maze.Matrix[i, j, 1])
                    {
                        DrawLine(startX + wallSize, startY, startX + wallSize, startY + wallSize, strokeSize, Brushes.Black);
                    }

                    //shift right after every cell
                    startX += wallSize;
                }
                //reposition x coordinate after a row is done
                startX = startX_Copy;

                //shift below after the row is done
                startY += wallSize;
            }
        }

        private void DrawLine(double x1, double y1, double x2, double y2, double strokeSize, Brush brush)
        {
            Line line = new Line();
            Thickness thickness = new Thickness(0, 0, 0, 0);
            line.Margin = thickness;
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = strokeSize;
            line.Stroke = brush;

            //cooridinates for the line
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;

            canvasMaze.Children.Add(line);

            if (maze.TotalCells <= 4000)
            {
                DrawCircle(x1, y1, strokeSize / 2.0, brush);
                DrawCircle(x2, y2, strokeSize / 2.0, brush);
            }
        }

        private void AddButton(double x, double y, double size, ButtonInfo info)
        {
            Button button = new Button();
            button.Background = Brushes.Transparent;
            button.BorderBrush = Brushes.Transparent;
            button.Width = size;
            button.Height = size;
            button.Tag = info;

            button.Click += Button_Click;

            canvasMaze.Children.Add(button);

            //set coordinates in canvas
            button.SetValue(Canvas.LeftProperty, x);
            button.SetValue(Canvas.TopProperty, y);
        }

        public struct ButtonInfo
        {
            public double Xcoord;
            public double Ycoord;
            public uint CellId;
            public double wallSize;
            public double strokeSize;
        }

        private void DrawCircle(double x, double y, double radius, Brush brush)
        {
            Ellipse circle = new Ellipse()
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = brush,
                Fill = brush,
                StrokeThickness = 3
            };

            canvasMaze.Children.Add(circle);

            //set coordinates in canvas
            circle.SetValue(Canvas.LeftProperty, x - radius);
            circle.SetValue(Canvas.TopProperty, y - radius);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ButtonInfo info = (ButtonInfo)button.Tag;
            nodes.Add(info.CellId);

            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(255, (byte)(info.Ycoord * info.Xcoord % 256), (byte)(info.CellId * info.strokeSize % 256), (byte)(info.Xcoord + info.Ycoord + info.CellId % 256)));

            DrawCircle(info.Xcoord + info.wallSize / 2, info.Ycoord + info.wallSize / 2, info.strokeSize * 2, brush); ;

            if (nodes.Count > 1)
            {
                uint start = nodes[nodes.Count - 2];
                uint end = nodes[nodes.Count - 1];

                if (start != end)
                {
                    List<uint> path = maze.TracePath_BFS(start, end, maze.BreadthFirstSearch(start));

                    if (path.Count >= 2)
                    {
                        DisplayPaths(path, brush);
                    }
                    else
                    {
                        nodes.RemoveAt(nodes.Count - 1);
                        MessageBox.Show("Not Reachable");
                    }
                }
            }
            else if (nodes.Count == 1)
            {
                List<uint> path = maze.TracePath_BFS(maze.Entry, nodes[0], maze.BreadthFirstSearch(maze.Entry));

                if (path.Count == 0)
                {
                    nodes.Clear();
                }
            }
        }

        private async void DisplayPaths(List<uint> paths, Brush brush)
        {
            if (paths.Count > 0)
            {
                double wallSize = (maze.Rows > maze.Columns ? canvasMaze.Height / maze.Rows : (maze.Rows == maze.Columns ? canvasMaze.Height / maze.Rows : canvasMaze.Height / maze.Rows));

                if (maze.Rows <= 10 && maze.Columns <= 10) wallSize = 65;

                double strokeSize = wallSize < 40 ? wallSize / 6.0 : 4;
                //stroke size for path is double the size of wall
                strokeSize = strokeSize * 2;

                double startX = (canvasMaze.Width / 2 - (wallSize * maze.Columns / 2)) + wallSize / 2;
                double startY = (canvasMaze.Height / 2 - (wallSize * maze.Rows / 2) + wallSize / 2);

                double startX_Original = startX;
                double startY_Original = startY;

                if (paths[0] == maze.Entry)
                {
                    //Draw line streched from outside to entry cell
                    DrawLine(startX - wallSize * 3, startY, startX, startY, strokeSize, brush);
                }

                int counter = 0;
                foreach (uint cellID in paths)
                {
                    await Task.Delay(10);

                    double centerX = startX_Original + (wallSize * maze.GetColumnFromCellID(cellID));
                    double centerY = startY_Original + (wallSize * maze.GetRowFromCellID(cellID));

                    if (counter > 0)
                    {
                        DrawLine(startX, startY, centerX, centerY, strokeSize, brush);
                    }

                    startX = centerX;
                    startY = centerY;

                    counter++;
                }

                if (paths[paths.Count - 1] == maze.Exit)
                {
                    //Draw line stretched from exit cell to outside
                    DrawLine(startX, startY, startX + wallSize * 3, startY, strokeSize, brush);
                }
            }
        }

        private string StringPaths()
        {
            string text = "";
            for (int i = 0; i < maze.Rows * maze.Columns; i++)
            {
                text += i + ": ";
                foreach (byte path in maze.Paths[i])
                {
                    text += path + " - ";
                }
                text = text.Substring(0, text.Length - 2);
                text += "\n";
            }
            return text;
        }

        private double WallSize()
        {
            double wallSize = (maze.Rows > maze.Columns ? canvasMaze.Height / maze.Rows : (maze.Rows == maze.Columns ? canvasMaze.Height / maze.Rows : canvasMaze.Height / maze.Rows));

            if (maze.Rows <= 10 && maze.Columns <= 10) wallSize = 65;

            return wallSize;
        }

        private double StrokeSize()
        {
            return WallSize() < 40 ? WallSize() / 6.0 : 4;
        }
        private double MazeStartX()
        {
            return (canvasMaze.Width / 2 - (WallSize() * maze.Columns / 2));
        }

        private double MazeStartY()
        {
            return (canvasMaze.Height / 2 - (WallSize() * maze.Rows / 2));
        }

        private double CellCenterX(uint cellID)
        {
            return MazeStartX() + (WallSize() * maze.GetColumnFromCellID(cellID)) + WallSize() / 2;
        }

        private double CellCenterY(uint cellID)
        {
            return MazeStartY() + (WallSize() * maze.GetRowFromCellID(cellID)) + WallSize() / 2;
        }

        private double CellStartX(uint cellID)
        {
            return MazeStartX() + (WallSize() * maze.GetColumnFromCellID(cellID));
        }

        private double CellStartY(uint cellID)
        {
            return MazeStartY() + (WallSize() * maze.GetRowFromCellID(cellID));
        }

        private void btnGenerateMaze_Click(object sender, RoutedEventArgs e)
        {
            nodes.Clear();
            maze = new Maze(200, 280);
            //maze.KnockDownWall(0, 0, 2);
            //maze.KnockDownWall(1, 0, 1);
            //maze.KnockDownWall(1, 1, 2);
            //maze.KnockDownWall(2, 2, 3);
            DisplayMaze();
            MessageBox.Show("ok");
            List<uint> path = maze.CreatePath_RandomWallDestroy();
            DisplayMaze();
            
            MessageBox.Show(String.Format("{0}X{1} Maze Generated", maze.Rows, maze.Columns));
            //string text = "";
            //foreach (uint p in path)
            //{
            //    text += p + "->";
            //}
            //MessageBox.Show(text);
            //maze.RandomlyKnockWalls(100);


            // DisplayPaths(path, Brushes.Teal);
            //MessageBox.Show("Go");


            //maze.ReconstructMaze_PathOnly(path);
            //path = maze.TracePath_BFS(2201, 34, maze.BreadthFirstSearch(2201));
            //DisplayMaze();

            //MessageBox.Show("Go");
            //path = maze.CreateOmnireachPaths();
            //DisplayMaze();

            //path = maze.FindSurroundingNeighborCells(a, 0);
            //string text = "";
            //DrawCircle(CellCenterX(a), CellCenterY(a), StrokeSize()*2, Brushes.Teal);
            //foreach (var item in path)
            //{
            //    DrawCircle(CellCenterX(item), CellCenterY(item), StrokeSize(), Brushes.Red);
            //}
            //DrawCircle(CellCenterX(12), CellCenterY(12), StrokeSize(), Brushes.Red);
            //MessageBox.Show("Neighbors: " + text);

            //MessageBox.Show((time-time2)+"ms");
            //string text = "";
            //foreach (uint cell in maze.DestroyableWalls)
            //{
            //    text += cell + " ";
            //}
            //DisplayMaze();
            //MessageBox.Show(text);

            //DisplayPaths(path,Brushes.Teal);

            //MessageBox.Show(StringPaths());
        }
    }
}
