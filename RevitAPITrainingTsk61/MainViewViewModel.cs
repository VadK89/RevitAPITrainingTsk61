using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;

namespace RevitAPITrainingTsk61
{
    /*Создайте приложение в WPF, которое создаёт воздуховод по двум введённым точкам. 
     * Тип воздуховода,уровень расположения должен выбираться из выпадающего списка. Смещение
     * воздуховода задаётся с помощью ввода значения в окне.*/
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public List<Level> Levels { get; } = new List<Level>();

        public DelegateCommand SaveCommand { get; }
        public double DuctOffset { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();

        public DuctType SelectedDuctType { get; set; }
        public Level SelectedLevel { get; set; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctsUtils.GetDuctTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            DuctOffset = 0;
            Points = SelectionUtils.GetPoints(_commandData, "Выберете точки", ObjectSnapTypes.Endpoints);

        }
        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            XYZ startPoint = null;
            XYZ endPoint = null;
            //проверка
            if (Points.Count < 2 || SelectedDuctType == null || SelectedLevel == null)
                return;
            //рисование по точкам
            var curves = new List<Curve>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;
                //сохранение точек
                var prevPoint = Points[i - 1];
                var currentPoint = Points[i];
                startPoint = prevPoint;
                endPoint = currentPoint;
                //создание линии
                Curve curve = Line.CreateBound(prevPoint, currentPoint);
                curves.Add(curve);
            }


            MEPSystemType SelectedsystemType = new FilteredElementCollector(doc)
                .OfClass(typeof(MEPSystemType))
                .Cast<MEPSystemType>()
                .FirstOrDefault(sysType => sysType.SystemClassification == MEPSystemClassification.SupplyAir);



            //создание воздуховодов
            using (var ts = new Transaction(doc, "Create wall"))
            {
                ts.Start();


                foreach (var curve in curves)
                {
                    startPoint = curve.GetEndPoint(0);
                    endPoint = curve.GetEndPoint(1);
                   Duct duct = Duct.Create(doc, SelectedsystemType.Id, SelectedDuctType.Id, SelectedLevel.Id, startPoint, endPoint);
                    if (duct is Duct)
                    {
                        Parameter offset = duct.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
                                              
                        offset.Set(DuctOffset);
                           
                    }

                }

                ts.Commit();

            }

            RaiseCloseRequest();
        }

        //для закрытия окна
        public event EventHandler CloseRequest;
        //метод для закрытия окна
        private void RaiseCloseRequest()
        {//Для запуска методов привзязанных к запросу закрытия
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }


    }
}
