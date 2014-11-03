using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Missile
{
    public class ComputeParams
    {
        /// <summary>
        /// Скорость цели (м/с)
        /// </summary>
        public double Vt { get; set; }
        /// <summary>
        /// Скорость полёта ракеты (м/с)
        /// </summary>
        public double Vm { get; set; }
        /// <summary>
        /// Высота полёта цели (м)
        /// </summary>
        public double H { get; set; }
        /// <summary>
        /// Угол наклона плоскости атаки к плоскости крыльев самолёта-цели (рад)
        /// </summary>
        public double alphaH { get; set; }
        /// <summary>
        /// Угол перехвата (рад)
        /// </summary>
        public double ae { get; set; }
        /// <summary>
        /// Скорость метания стержней
        /// </summary>
        public double Vo { get; set; }
        /// <summary>
        /// Радиус полностью раскрывшегося стержневого кольца (м)
        /// </summary>
        public double Rmax { get; set; }
        /// <summary>
        /// Диаметр стержня (мм)
        /// </summary>
        public double d { get; set; }
        /// <summary>
        /// Масса взрывчатого вещества в пересчёте на тротил (кг)
        /// </summary>
        public double q { get; set; }
        /// <summary>
        /// Угол наклона вектора Vo к прордольной оси ракеты (рад)
        /// </summary>
        public double phi { get; set; }
        /// <summary>
        /// Дальность действия неконтактного взрывателя (м)
        /// </summary>
        public double D { get; set; }
        /// <summary>
        /// Угол наклона главного лепестка диаграммы направленности (рад)
        /// </summary>
        public double gamma { get; set; }
        /// <summary>
        /// Временная задержка взрыва (с)
        /// </summary>
        public double tau { get; set; }
        /// <summary>
        /// Среднее квадратическое отклонение рассеивания точки взрыва вдоль относительной траектории (м)
        /// </summary>
        public double sigmaX2 { get; set; }
        /// <summary>
        /// Систематическая ошибка по X (м)
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// Систематическая ошибка по Y (м)
        /// </summary>
        public double y { get; set; }
        /// <summary>
        /// Среднее квадратическое отклонение
        /// </summary>
        public double sigmaX { get; set; }
        /// <summary>
        /// Среднее квадратическое отклонение
        /// </summary>
        public double sigmaY { get; set; }
    }
}
