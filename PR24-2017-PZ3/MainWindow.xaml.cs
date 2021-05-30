using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PR24_2017_PZ3.Model;
using System.Xml;
using System.Windows.Media.Media3D;
using Point = System.Windows.Point;
using System.Collections;

namespace PR24_2017_PZ3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    

    public partial class MainWindow : Window
    {

        Dictionary<long, SubstationEntity> subEnt = new Dictionary<long, SubstationEntity>();
        Dictionary<long, NodeEntity> nodeEnt = new Dictionary<long, NodeEntity>();
        Dictionary<long, SwitchEntity> swcEnt = new Dictionary<long, SwitchEntity>();
        Dictionary<long, LineEntity> lineEnt = new Dictionary<long, LineEntity>();
        Dictionary<long, LineEntity> lineEntDraw = new Dictionary<long, LineEntity>();

        private bool show = true;

       
        private static int cubeSize = 5;

        double minLat = 45.2325;
        double maxLat = 45.277031;
        double minLon = 19.793909;
        double maxLon = 19.894459;


        private Point start = new Point();
        private Point Rstart = new Point();
        private Point diffOffset = new Point();
        private int zoomMax = 100;
        private int zoomCurent = 1;
        

        private static int sizeX = 1175 / cubeSize;  //mora biti deljivo sa velicinom stranice kocke!
        private static int sizeY = 775 / cubeSize;
        double[,] pointMatrix = new double[sizeX+1, sizeY+1];  //matrica u kojoj ce se cuvati sta je na kojoj poziciji
                                                               //ako se na poziciji nalazi jedna kocka bice vrednost 1, 
                                                               //kasnije se povecava vrednost za 1 za svaku kocku na isto poziciji

        TextBlock toolTip = new TextBlock();
        double toolTipX;
        double toolTipY;

        private GeometryModel3D hitgeo;
        private Dictionary<long, GeometryModel3D> ViewPortModels = new Dictionary<long, GeometryModel3D>();
        private List<IDLine> drawnLines = new List<IDLine>();

        public MainWindow()
        {
            InitializeComponent();

           
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList nodeList;
            XmlNodeList nodeList2;
            XmlNodeList nodeList3;
            XmlNodeList nodeList4;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            nodeList2 = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            nodeList3 = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            nodeList4 = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");

            Button Show = new Button();
            Show.Content = "Show";
            Show.Width = 50;
            Show.Height = 30;
            Show.Margin = new Thickness(750, 5, 0, 700);
            Show.Click += Show_Click;
            grid.Children.Add(Show);

            Button Hide = new Button();
            Hide.Content = "Hide";
            Hide.Width = 50;
            Hide.Height = 30;
            Hide.Margin = new Thickness(870, 5, 0, 700);
            Hide.Click += Hide_Click;
            grid.Children.Add(Hide);



            for (int i=0; i<sizeX; i++)
            {
                for(int j=0; j<sizeY; j++)
                {
                    pointMatrix[i, j] = 0;
                }
            }


            foreach (XmlNode node in nodeList)
            {
                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                //sub.X = CalculateXCoord(sub.X, minX, maxX);
                //sub.Y = CalculateYCoord(sub.Y, minY, maxY);

                double newX, newY;

                ToLatLon(sub.X, sub.Y, 34, out double latitude, out double longitude);

                //checkMatrix(sub.X, sub.Y, out newX, out newY);
                sub.X = longitude;
                sub.Y = latitude;

                //pointMatrix[(int)sub.X, (int)sub.Y] = 1;

                if ((sub.Y >= 45.2325 && sub.Y <= 45.277031) && (sub.X >= 19.793909 && sub.X <= 19.894459))
                {
                    subEnt.Add(sub.Id, sub);
                }

            }

            foreach (XmlNode node in nodeList2)
            {
                NodeEntity nodeobj = new NodeEntity();
                nodeobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeobj.Name = node.SelectSingleNode("Name").InnerText;
                nodeobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                //nodeobj.X = CalculateXCoord(nodeobj.X, minX, maxX);
                //nodeobj.Y = CalculateYCoord(nodeobj.Y, minY, maxY);

                double newX, newY;

                ToLatLon(nodeobj.X, nodeobj.Y, 34, out double latitude, out double longitude);

                //checkMatrix(nodeobj.X, nodeobj.Y, out newX, out newY);
                nodeobj.X = longitude;
                nodeobj.Y = latitude;

                //pointMatrix[(int)nodeobj.X, (int)nodeobj.Y] = 1;

                if ((nodeobj.Y >= 45.2325 && nodeobj.Y <= 45.277031) && (nodeobj.X >= 19.793909 && nodeobj.X <= 19.894459))
                {
                    nodeEnt.Add(nodeobj.Id, nodeobj);
                }

            }

            foreach (XmlNode node in nodeList3)
            {
                SwitchEntity switchobj = new SwitchEntity();
                switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchobj.Name = node.SelectSingleNode("Name").InnerText;
                switchobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                switchobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchobj.Status = node.SelectSingleNode("Status").InnerText;

                //switchobj.X = CalculateXCoord(switchobj.X, minX, maxX);
                //switchobj.Y = CalculateYCoord(switchobj.Y, minY, maxY);

                double newX, newY;

                ToLatLon(switchobj.X, switchobj.Y, 34, out double latitude, out double longitude);

                //checkMatrix(switchobj.X, switchobj.Y, out newX, out newY);
                switchobj.X = longitude;
                switchobj.Y = latitude;

                //pointMatrix[(int)switchobj.X, (int)switchobj.Y] = 1;

                if ((switchobj.Y >= 45.2325 && switchobj.Y <= 45.277031) && (switchobj.X >= 19.793909 && switchobj.X <= 19.894459))
                {
                    swcEnt.Add(switchobj.Id, switchobj);
                }

            }

            foreach (XmlNode node in nodeList4)
            {
                LineEntity l = new LineEntity();
                List<Point> points = new List<Point>();

                l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                l.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    l.IsUnderground = true;
                }
                else
                {
                    l.IsUnderground = false;
                }
                l.R = float.Parse(node.SelectSingleNode("R").InnerText);
                l.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                l.LineType = node.SelectSingleNode("LineType").InnerText;
                l.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);
                l.Vertices = new List<Model.Point>();

                foreach (XmlNode pointNode in node.ChildNodes[9].ChildNodes) // 9 posto je Vertices 9. node u jednom line objektu
                {
                    Point p = new Point();

                    p.X = double.Parse(pointNode.SelectSingleNode("X").InnerText);
                    p.Y = double.Parse(pointNode.SelectSingleNode("Y").InnerText);

                    ToLatLon(p.X, p.Y, 34, out double noviX, out double noviY);

                    l.Vertices.Add(new Model.Point() {X = noviX, Y = noviY });
                }

                lineEnt.Add(l.Id, l);

            }

            drawSubstations();
            drawNodestations();
            drawSwcstations();
            checkLines();
            drawLines();


        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void drawSubstations()
        {
            foreach (SubstationEntity sub in subEnt.Values)
            {

                ModelVisual3D subStat = new ModelVisual3D(); ;

                GeometryModel3D geoMod = new GeometryModel3D();
             
                
                MeshGeometry3D meshGeo = new MeshGeometry3D();

                int x1 = calculateX(sub.X);
                int y1 = calculateY(sub.Y);

                int x = x1 * cubeSize;   //mnozim sa cubeSize da dobijem pravu koordinatu na kojoj treba biti nacrtana kocka
                int y = y1 * cubeSize;

                int value = (int)pointMatrix[x1, y1];

                meshGeo.Positions = new Point3DCollection {new Point3D(x, y, value*cubeSize), new Point3D(x+cubeSize, y, value*cubeSize) , new Point3D(x, y+cubeSize, value*cubeSize), new Point3D(x+cubeSize, y+cubeSize, value*cubeSize),
                                                       new Point3D(x, y, cubeSize+(value*cubeSize)), new Point3D(x+cubeSize, y, cubeSize+(value*cubeSize)), new Point3D(x, y+cubeSize, cubeSize+(value*cubeSize)), new Point3D(x+cubeSize, y+cubeSize, cubeSize+(value*cubeSize))};

                meshGeo.TriangleIndices = new Int32Collection { 0, 1, 5, 0, 5, 4, 1, 3, 7, 1, 7, 5, 3, 2, 6, 3, 6, 7, 2, 0, 4, 2, 4, 6, 4, 5, 7, 4, 7, 6, 2, 3, 1, 2, 1, 0};

                geoMod.Geometry = meshGeo;

                pointMatrix[x1, y1] = value + 1;   //update vrednosti, nacrtala se kocka povecavamo za jedan na tom polju

                geoMod.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
                

                subStat.Content = geoMod;
                

                models.Children.Add(geoMod);
                ViewPortModels.Add(sub.Id, geoMod);



            }
        }

        private void drawNodestations()
        {
            foreach (NodeEntity nod in nodeEnt.Values)
            {

                ModelVisual3D subStat = new ModelVisual3D(); ;

                GeometryModel3D geoMod = new GeometryModel3D();

                MeshGeometry3D meshGeo = new MeshGeometry3D();

                int x1 = calculateX(nod.X);
                int y1 = calculateY(nod.Y);

                int x = x1 * cubeSize;   //mnozim sa cubeSize da dobijem pravu koordinatu na kojoj treba biti nacrtana kocka
                int y = y1 * cubeSize;

                int value = (int)pointMatrix[x1, y1];

                meshGeo.Positions = new Point3DCollection {new Point3D(x, y, value*cubeSize), new Point3D(x+cubeSize, y, value*cubeSize) , new Point3D(x, y+cubeSize, value*cubeSize), new Point3D(x+cubeSize, y+cubeSize, value*cubeSize),
                                                       new Point3D(x, y, cubeSize+(value*cubeSize)), new Point3D(x+cubeSize, y, cubeSize+(value*cubeSize)), new Point3D(x, y+cubeSize, cubeSize+(value*cubeSize)), new Point3D(x+cubeSize, y+cubeSize, cubeSize+(value*cubeSize))};

                meshGeo.TriangleIndices = new Int32Collection { 0, 1, 5, 0, 5, 4, 1, 3, 7, 1, 7, 5, 3, 2, 6, 3, 6, 7, 2, 0, 4, 2, 4, 6, 4, 5, 7, 4, 7, 6, 2, 3, 1, 2, 1, 0 };

                geoMod.Geometry = meshGeo;


                geoMod.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));


                subStat.Content = geoMod;

                pointMatrix[x1, y1] = value + 1;   //update vrednosti, nacrtala se kocka povecavamo za jedan na tom polju
                

                models.Children.Add(geoMod);
                ViewPortModels.Add(nod.Id, geoMod);


            }
        }

        private void drawSwcstations()
        {
            foreach (SwitchEntity swc in swcEnt.Values)
            {

                ModelVisual3D subStat = new ModelVisual3D(); ;

                GeometryModel3D geoMod = new GeometryModel3D();

                MeshGeometry3D meshGeo = new MeshGeometry3D();

                int x1 = calculateX(swc.X);
                int y1 = calculateY(swc.Y);

                int x = x1 * cubeSize;   //mnozim sa cubeSize da dobijem pravu koordinatu na kojoj treba biti nacrtana kocka
                int y = y1 * cubeSize;

                int value = (int)pointMatrix[x1, y1];

                meshGeo.Positions = new Point3DCollection {new Point3D(x, y, value*cubeSize), new Point3D(x+cubeSize, y, value*cubeSize) , new Point3D(x, y+cubeSize, value*cubeSize), new Point3D(x+cubeSize, y+cubeSize, value*cubeSize),
                                                       new Point3D(x, y, cubeSize+(value*cubeSize)), new Point3D(x+cubeSize, y, cubeSize+(value*cubeSize)), new Point3D(x, y+cubeSize, cubeSize+(value*cubeSize)), new Point3D(x+cubeSize, y+cubeSize, cubeSize+(value*cubeSize))};


                meshGeo.TriangleIndices = new Int32Collection { 0, 1, 5, 0, 5, 4, 1, 3, 7, 1, 7, 5, 3, 2, 6, 3, 6, 7, 2, 0, 4, 2, 4, 6, 4, 5, 7, 4, 7, 6, 2, 3, 1, 2, 1, 0 };

                geoMod.Geometry = meshGeo;


                geoMod.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));


                subStat.Content = geoMod;

                pointMatrix[x1, y1] = value + 1;   //update vrednosti, nacrtala se kocka povecavamo za jedan na tom polju
                
                
                models.Children.Add(geoMod);
                ViewPortModels.Add(swc.Id, geoMod);


            }
        }

        private int calculateX(double longitude)
        {
            int retVal = 0;

            double diff = maxLon - minLon;
            double num = longitude - minLon;
            double finalNum = diff / num;

            retVal = (int)Math.Round(sizeX / finalNum);

            return retVal;
        }

        private int calculateY(double latitude)
        {
            int retVal = 0;

            double diff = maxLat - minLat;
            double num = latitude - minLat;
            double finalNum = diff / num;

            retVal = (int)Math.Round(sizeY / finalNum);

            return retVal;
        }


        private void MouseWheelZoom(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(this);
            double scaleX = 1;
            double scaleY = 1;
            double scaleZ = 1;
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                scaleX = skaliranje.ScaleX + 0.1;
                scaleY = skaliranje.ScaleY + 0.1;
                scaleZ = skaliranje.ScaleZ + 0.1;
                zoomCurent++;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
                skaliranje.ScaleZ = scaleZ;
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax && !(zoomCurent == 1))
            {
                scaleX = skaliranje.ScaleX - 0.1;
                scaleY = skaliranje.ScaleY - 0.1;
                scaleZ = skaliranje.ScaleZ - 0.1;
                zoomCurent--;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
                skaliranje.ScaleZ = scaleZ;

            }
        }

        private void MouseMoveVP(object sender, MouseEventArgs e)
        {
            if (viewPort.IsMouseCaptured)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 100000) / w;
                double translateY = -(offsetY * 100000) / h;
                translacija.OffsetX = diffOffset.X + (translateX / (100 * skaliranje.ScaleX));
                translacija.OffsetY = diffOffset.Y + (translateY / (100 * skaliranje.ScaleX));
                
            }

            if(e.MiddleButton == MouseButtonState.Pressed)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - Rstart.X;
                double offsetY = end.Y - Rstart.Y;
                rotateY.Angle += offsetX/2;
                rotateX.Angle += offsetY/2;
                Rstart = end;
            }
        }

        private void MouseLeftButtonDownVP(object sender, MouseButtonEventArgs e)
        {
            grid.Children.Remove(toolTip);

            viewPort.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;

            ////HIT TEST
            System.Windows.Point mouseposition = e.GetPosition(viewPort);
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

            PointHitTestParameters pointparams =
                     new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams =
                     new RayHitTestParameters(testpoint3D, testdirection);

            //test for a result in the Viewport3D     
            hitgeo = null;
            VisualTreeHelper.HitTest(viewPort, null, HTResult, pointparams);

        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {

            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                
                bool gasit = false;
                for (int i = 0; i < ViewPortModels.Count; i++)
                {
                    if ((GeometryModel3D)ViewPortModels.ElementAt(i).Value == rayResult.ModelHit)
                    {
                        hitgeo = (GeometryModel3D)rayResult.ModelHit;
                        gasit = true;
                        PowerEntity ent = FindEntity(ViewPortModels.ElementAt(i).Key);
                        toolTipX = start.X;
                        toolTipY = start.Y;

                        if(toolTipY < 404)
                        {
                            toolTipY -= 70;
                        }

                        if(toolTipX < 700 && toolTipX > 500)
                        {
                            toolTipX = 400;
                        }else if (toolTipX < 500 && toolTipX > 300)
                        {
                            toolTipX = 200;
                        }
                        else if (toolTipX < 300 && toolTipX > 100)
                        {
                            toolTipX = 100;
                        }else if(toolTipX > 700)
                        {
                            toolTipX += 200;
                        }

                        toolTip.Text = "Name: " + ent.Name + " ID: " + ent.Id.ToString() + "\nTip: " + FindType(ent);
                        toolTip.Foreground = Brushes.Black;
                        toolTip.Background = Brushes.White;
                        toolTip.Width = 300;
                        toolTip.Height = 30;
                        toolTip.Margin = new Thickness(toolTipX-680, toolTipY-300, 0, 0);
                        grid.Children.Add(toolTip);
                        Grid.SetRow(toolTip, 0);
                        Grid.SetColumn(toolTip, 0);

                    }
                    
                }

                for (int i = 0; i < drawnLines.Count; i++)
                {
                    if ((GeometryModel3D)drawnLines.ElementAt(i).LineSeg == rayResult.ModelHit)
                    {
                        hitgeo = (GeometryModel3D)rayResult.ModelHit;
                        gasit = true;
                        ChangeColor(drawnLines.ElementAt(i).Id);
                    }
                }
                if (!gasit)
                {
                    hitgeo = null;
                }
            }

            return HitTestResultBehavior.Stop;
        }

        private void ChangeColor(long id)
        {
            if (lineEnt.ContainsKey(id))
            {
                LineEntity line = lineEnt[id];

                long firstId = IdHelp(line.FirstEnd);
                long secondId = IdHelp(line.SecondEnd);

                if(firstId != 0 && secondId != 0 && ViewPortModels.ContainsKey(firstId) && ViewPortModels.ContainsKey(secondId))
                {
                    GeometryModel3D first = ViewPortModels[firstId];
                    GeometryModel3D second = ViewPortModels[secondId];

                    GeometryModel3D firstCh = new GeometryModel3D();
                    GeometryModel3D secondCh = new GeometryModel3D();

                    for(int i=0; i<models.Children.Count; i++)
                    {
                        if(models.Children.ElementAt(i) == first)
                        {
                            models.Children.RemoveAt(i);
                            first.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
                            ViewPortModels[firstId].Material = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
                            models.Children.Insert(i, first);
                        }
                        if (models.Children.ElementAt(i) == second)
                        {
                            models.Children.RemoveAt(i);
                            second.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
                            ViewPortModels[secondId].Material = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
                            models.Children.Insert(i, second);
                        }
                    }
                }
            }
        }

        private long IdHelp(long id)
        {
            if (swcEnt.ContainsKey(id))
            {
                return swcEnt[id].Id;

            }else if (nodeEnt.ContainsKey(id))
            {
                return nodeEnt[id].Id;

            }else if (subEnt.ContainsKey(id))
            {
                return subEnt[id].Id;
            }

            return 0;
        }

        private PowerEntity FindEntity(long key)
        {
            if (swcEnt.ContainsKey(key))
            {
                return swcEnt[key];
            }
            if (nodeEnt.ContainsKey(key))
            {
                return nodeEnt[key];
            }
            if (subEnt.ContainsKey(key))
            {
                return subEnt[key];
            }

            return null;
        }

        private string FindType(PowerEntity ent)
        {
            if (swcEnt.ContainsKey(ent.Id))
            {
                return "SwitchEntity";
            }
            if (nodeEnt.ContainsKey(ent.Id))
            {
                return "NodeEntity";
            }
            if (subEnt.ContainsKey(ent.Id))
            {
                return "SubstationEntity";
            }

            return null;
        }

        private void checkLines()  ///crtam samo linije koje imaju cvorove na mapi
        {
            foreach(LineEntity line in lineEnt.Values)
            {
                if ((ViewPortModels.ContainsKey(line.FirstEnd) && ViewPortModels.ContainsKey(line.SecondEnd)))
                {
                    lineEntDraw.Add(line.Id, line);
                }
            }
        }

        private void drawLines()
        {
            foreach(LineEntity line in lineEntDraw.Values)
            {
                for(int i=0; i<line.Vertices.Count-1; i++)
                {
                    for(int j=1; j<line.Vertices.Count; j++)
                    {
                        drawLineSegment(line.Vertices[i], line.Vertices[j], line.Id);
                        i++;
                    }
                }
            }
        }

        private void drawLineSegment(Model.Point point1, Model.Point point2, long id)
        {
           ModelVisual3D subStat = new ModelVisual3D(); ;

            GeometryModel3D geoMod = new GeometryModel3D();

            MeshGeometry3D meshGeo = new MeshGeometry3D();

            int x = calculateX(point1.Y);
            int y = calculateY(point1.X);

            int x1 = x * cubeSize;   //mnozim sa cubeSize da dobijem pravu koordinatu na kojoj treba biti nacrtana kocka
            int y1 = y * cubeSize;

            x = calculateX(point2.Y);
            y = calculateY(point2.X);

            int x2 = x * cubeSize;   //mnozim sa cubeSize da dobijem pravu koordinatu na kojoj treba biti nacrtana kocka
            int y2 = y * cubeSize;


            if(x1 > x2)
            {
                int tempX = x1;
                int tempY = y1;
                x1 = x2;
                x2 = tempX;
                y1 = y2;
                y2 = tempY;
            }

            if(y2 > y1)
            {

                int tempX = x1;
                int tempY = y1;
                x1 = x2;
                x2 = tempX;
                y1 = y2;
                y2 = tempY;
            }

            if ((x2 - x1) < 15 && (Math.Abs(y1 - y2) > (x2 - x1)))
            {
                meshGeo.Positions = new Point3DCollection {new Point3D(x1, y1, 0), new Point3D(x2, y2, 0) , new Point3D(x1+cubeSize, y1, 0), new Point3D(x2+cubeSize, y2, cubeSize),
                                                       new Point3D(x1, y1, cubeSize-2), new Point3D(x2, y2, cubeSize-2), new Point3D(x1+cubeSize, y1, cubeSize-2), new Point3D(x2+cubeSize, y2, cubeSize-2)};
                meshGeo.TriangleIndices = new Int32Collection { 0, 2, 6, 0, 6, 4, 2, 3, 7, 2, 7, 6, 4, 6, 7, 4, 7, 5, 3, 1, 5, 3, 5, 7, 1, 3, 2, 1, 2, 0, 1, 0, 4, 1, 4, 5 };

            }
            else
            {
                meshGeo.Positions = new Point3DCollection {new Point3D(x1, y1, 0), new Point3D(x2, y2, 0) , new Point3D(x1, y1+cubeSize, 0), new Point3D(x2, y2+cubeSize, cubeSize),
                                                       new Point3D(x1, y1, cubeSize-2), new Point3D(x2, y2, cubeSize-2), new Point3D(x1, y1+cubeSize, cubeSize-2), new Point3D(x2, y2+cubeSize, cubeSize-2)};
                meshGeo.TriangleIndices = new Int32Collection { 0, 1, 5, 0, 5, 4, 1, 3, 7, 1, 7, 5, 3, 2, 6, 3, 6, 7, 2, 0, 4, 2, 4, 6, 4, 5, 7, 4, 7, 6, 2, 3, 1, 2, 1, 0 };


            }


            geoMod.Geometry = meshGeo;


            geoMod.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));


            subStat.Content = geoMod;

            models.Children.Add(geoMod);
            drawnLines.Add(new IDLine() { Id = id, LineSeg = geoMod });
            //ViewPortModels.Add(swc.Id, geoMod);

        }

        private void MouseLeftButtonUpVP(object sender, MouseButtonEventArgs e)
        {
            viewPort.ReleaseMouseCapture();
        }

        private void MouseDownVP(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Pressed)
            {
                Rstart = e.GetPosition(this);
            }
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            if(show == true)
            {
                return;
            }
            else
            {
                foreach(IDLine geoMod in drawnLines)
                {
                    models.Children.Add(geoMod.LineSeg);
                }
            }
            show = true;
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            if (show == false)
            {
                return;
            }
            else
            {
                foreach (IDLine geoMod in drawnLines)
                {
                    models.Children.Remove(geoMod.LineSeg);
                }
            }

            show = false;
        }
    }
}
