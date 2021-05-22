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

namespace PR24_2017_PZ3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<long, SubstationEntity> subEnt = new Dictionary<long, SubstationEntity>();
        Dictionary<long, NodeEntity> nodeEnt = new Dictionary<long, NodeEntity>();
        Dictionary<long, SwitchEntity> swcEnt = new Dictionary<long, SwitchEntity>();
        Dictionary<long, LineEntity> lineEnt = new Dictionary<long, LineEntity>();

        int sizeX = 1175; 
        int sizeY = 775;

        int cubeSize = 5;

        double minLat = 45.2325;
        double maxLat = 45.277031;
        double minLon = 19.793909;
        double maxLon = 19.894459;


        private Point start = new Point();
        private Point diffOffset = new Point();
        private int zoomMax = 7;
        private int zoomCurent = 1;



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

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            nodeList2 = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            nodeList3 = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");


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

            drawSubstations();
            drawNodestations();
            drawSwcstations();


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

                int x = calculateX(sub.X);
                int y = calculateY(sub.Y);

                meshGeo.Positions = new Point3DCollection {new Point3D(x, y, 0), new Point3D(x+cubeSize, y, 0) , new Point3D(x, y+cubeSize, 0), new Point3D(x+cubeSize, y+cubeSize, 0),
                                                       new Point3D(x, y, 0), new Point3D(x+cubeSize, y, cubeSize), new Point3D(x, y+cubeSize, cubeSize), new Point3D(x+cubeSize, y+cubeSize, cubeSize)};

                meshGeo.TriangleIndices = new Int32Collection { 2, 3, 1, 3, 1, 0, 7, 1, 3, 7, 5, 1, 6, 5, 7, 6, 4, 5, 6, 2, 0, 2, 0, 4, 2, 7, 3, 2, 6, 7, 0, 1, 5, 0, 5, 4, };

                geoMod.Geometry = meshGeo;
                

                geoMod.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
                

                subStat.Content = geoMod;

                //viewPort.Children.Add(subStat);

                models.Children.Add(geoMod);

                
            }
        }

        private void drawNodestations()
        {
            foreach (NodeEntity nod in nodeEnt.Values)
            {

                ModelVisual3D subStat = new ModelVisual3D(); ;

                GeometryModel3D geoMod = new GeometryModel3D();

                MeshGeometry3D meshGeo = new MeshGeometry3D();

                int x = calculateX(nod.X);
                int y = calculateY(nod.Y);

                meshGeo.Positions = new Point3DCollection {new Point3D(x, y, 0), new Point3D(x+cubeSize, y, 0) , new Point3D(x, y+cubeSize, 0), new Point3D(x+cubeSize, y+cubeSize, 0),
                                                       new Point3D(x, y, 0), new Point3D(x+cubeSize, y, cubeSize), new Point3D(x, y+cubeSize, cubeSize), new Point3D(x+cubeSize, y+cubeSize, cubeSize)};

                meshGeo.TriangleIndices = new Int32Collection { 2, 3, 1, 3, 1, 0, 7, 1, 3, 7, 5, 1, 6, 5, 7, 6, 4, 5, 6, 2, 0, 2, 0, 4, 2, 7, 3, 2, 6, 7, 0, 1, 5, 0, 5, 4, };

                geoMod.Geometry = meshGeo;


                geoMod.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));


                subStat.Content = geoMod;

                //viewPort.Children.Add(subStat);

                models.Children.Add(geoMod);


            }
        }

        private void drawSwcstations()
        {
            foreach (SwitchEntity swc in swcEnt.Values)
            {

                ModelVisual3D subStat = new ModelVisual3D(); ;

                GeometryModel3D geoMod = new GeometryModel3D();

                MeshGeometry3D meshGeo = new MeshGeometry3D();

                int x = calculateX(swc.X);
                int y = calculateY(swc.Y);

                meshGeo.Positions = new Point3DCollection {new Point3D(x, y, 0), new Point3D(x+cubeSize, y, 0) , new Point3D(x, y+cubeSize, 0), new Point3D(x+cubeSize, y+cubeSize, 0),
                                                       new Point3D(x, y, 0), new Point3D(x+cubeSize, y, cubeSize), new Point3D(x, y+cubeSize, cubeSize), new Point3D(x+cubeSize, y+cubeSize, cubeSize)};

                meshGeo.TriangleIndices = new Int32Collection { 2, 3, 1, 3, 1, 0, 7, 1, 3, 7, 5, 1, 6, 5, 7, 6, 4, 5, 6, 2, 0, 2, 0, 4, 2, 7, 3, 2, 6, 7, 0, 1, 5, 0, 5, 4, };

                geoMod.Geometry = meshGeo;


                geoMod.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));


                subStat.Content = geoMod;

                //viewPort.Children.Add(subStat);

                models.Children.Add(geoMod);


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
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                scaleX = skaliranje.ScaleX + 0.1;
                scaleY = skaliranje.ScaleY + 0.1;
                zoomCurent++;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            {
                scaleX = skaliranje.ScaleX - 0.1;
                scaleY = skaliranje.ScaleY - 0.1;
                zoomCurent--;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
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
        }

        private void MouseLeftButtonDownVP(object sender, MouseButtonEventArgs e)
        {
            viewPort.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;
        }

        private void MouseLeftButtonUpVP(object sender, MouseButtonEventArgs e)
        {
            viewPort.ReleaseMouseCapture();
        }
    }
}
