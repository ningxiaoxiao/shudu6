using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace shudu6
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var sw = new Stopwatch();
            sw.Start();

            var s = new Sudoku();

            /*
            //no7
            s.Add(0, "005042");
            s.Add(1, "000013");
            s.Add(2, "006000");
            s.Add(3, "000100");
            s.Add(4, "630000");
            s.Add(5, "250400");
            */

            /*
            //no8
            s.Add(0, "000000");
            s.Add(1, "064250");
            s.Add(2, "500004");
            s.Add(3, "300006");
            s.Add(4, "051430");
            s.Add(5, "000000");
            */


            // no9
            s.Add(0, "043000");
            s.Add(1, "000500");
            s.Add(2, "010600");
            s.Add(3, "004020");
            s.Add(4, "001000");
            s.Add(5, "000130");

            /*
            //no10
            s.Add(0, "020006");
            s.Add(1, "400000");
            s.Add(2, "001600");
            s.Add(3, "002400");
            s.Add(4, "000005");
            s.Add(5, "500060");
            */
            /*
            //no11
            s.Add(0, "102000");
            s.Add(1, "000020");
            s.Add(2, "010360");
            s.Add(3, "035010");
            s.Add(4, "020000");
            s.Add(5, "000402");
            */


            Console.WriteLine("原始问题:\r\n" + s);
            s.Solution();
            sw.Stop();
            Console.WriteLine($"共花费时间[{sw.ElapsedMilliseconds}]ms 计算[{Sudoku.SolutionCount}]次 找到解为:\r\n{Sudoku.SolutionSring}");
            Console.ReadKey();
        }
    }
    public class Sudoku
    {
        public static int SolutionCount;
        public static string SolutionSring;
        public static object Locker { get; } = new object();
        private static bool _findout;
        private int _deth;
        private int[,] _map = new int[6, 6];
        private string _description = "原始";
        private bool _noSolution;
        private bool Complete
        {
            get
            {
                for (var r = 0; r < 6; r++)
                {
                    for (var c = 0; c < 6; c++)
                    {
                        if (_map[r, c] == 0)
                            return false;
                    }
                }
                return true;
            }
        }
        private string Output
        {
            get
            {
                var ret = "┌───┬───┐\r\n";
                for (var r = 0; r < 6; r++)
                {
                    ret += "│ ";
                    for (var c = 0; c < 6; c++)
                    {
                        ret += _map[r, c] + (c == 2 ? "│" : " ");
                    }

                    ret += "│\r\n";
                    if (r == 1 || r == 3) ret += "├───┼───┤\r\n";
                }

                return ret + "└───┴───┘\r\n";
            }
        }
        public void Add(int r, string s)
        {
            var nums = Sting2Nums(s);
            for (var i = 0; i < 6; i++)
            {
                _map[r, i] = nums[i];
            }
        }
        public void Solution()
        {
            Log("开始解");
            SolutionCount++;
            bool onealldone;
            var findonedeth = 0;
            do
            {
                onealldone = Findone();
                findonedeth++;
            } while (!onealldone);
            Log($"一层解共循环{findonedeth}次");
            if (_noSolution)
                return;

            if (Complete)
            {
                _findout = true;
                SolutionSring = Output;

                Log("找到解", ConsoleColor.Red);
                return;
            }

            Findtwo();
        }
        /// <summary>
        /// 找出所有惟一解, 找不到 返回真
        /// </summary>
        /// <returns></returns>
        private bool Findone()
        {
            var onecount = 0;
            for (var r = 0; r < 6; r++)
            {
                for (var i = 0; i < 6; i++)
                {
                    var s = GetCrossPossibility(r, i);
                    if (s == null) continue;

                    if (s.Length == 0)
                    {
                        Log("无解");
                        _noSolution = true;
                        return true;
                    }
                    if (s.Length != 1) continue;
                    _map[r, i] = s[0];
                    onecount++;
                }
            }
            return onecount == 0;
        }
        private void Findtwo()
        {
            for (var r = 0; r < 6; r++)
            {
                for (var i = 0; i < 6; i++)
                {
                    if (_findout)
                    {
                        Log("已经找到解");
                        return;
                    }
                    var s = GetCrossPossibility(r, i);
                    if (s?.Length != 2) continue;
                    foreach (var num in s)
                    {
                        //分出两个版本 尝试进行解
                        var one = new Sudoku
                        {
                            _deth = _deth++,
                            _map = (int[,])_map.Clone()
                        };
                        one._map[r, i] = num;
                        one._description = $"[{r},{i}] 处 [{num}] 可能性";
                        one.Solution();
                        if (_findout)
                            return;


                    }
                }
            }
        }
        private static int[] Sting2Nums(string s)
        {
            if (s.Length != 6) throw new Exception("应该是6个数字");
            var ret = new int[6];
            
            var i = 0;
            foreach (var mumstr in s)
            {
                ret[i] = int.Parse(mumstr.ToString());
                i++;
            }

            return ret;
        }
        private int[] GetCrossPossibility(int r, int c)
        {
            //计算总共是不是有5个数字
            if (_map[r, c] != 0)
                return null;
            var rowMissd = GetRowMiss(r);
            var col = GetCol(c);
            var box = GetBox(r, c);

            foreach (var i in col)
            {
                rowMissd.Remove(i);
            }

            foreach (var i in box)
            {
                rowMissd.Remove(i);
            }

            return rowMissd.ToArray();
        }
        private List<int> GetRowMiss(int r)
        {
            var rows = GetRow(r);
            var miss = new List<int>();
            for (var i = 1; i < 7; i++)
            {
                if (!rows.Contains(i)) miss.Add(i);
            }
            return miss;
        }
        private int[] GetRow(int r)
        {
            var ret = new int[6];
            for (var i = 0; i < 6; i++)
            {
                ret[i] = _map[r, i];
            }
            return ret;
        }
        private int[] GetBox(int r, int c)
        {
            var box = new int[6];
            var nextrow = -1;
            switch (r)
            {
                case 0:
                    nextrow = 1;
                    break;

                case 1:
                    nextrow = 0;
                    break;

                case 2:
                    nextrow = 3;
                    break;

                case 3:
                    nextrow = 2;
                    break;

                case 4:
                    nextrow = 5;
                    break;

                case 5:
                    nextrow = 4;
                    break;
            }

            var startCol = c > 2 ? 3 : 0;
            for (var i = 0; i < 3; i++)
            {
                box[i] = (_map[r, startCol + i]);
                box[i + 3] = (_map[nextrow, startCol + i]);
            }
            return box;
        }
        private int[] GetCol(int c)
        {
            var ret = new int[6];
            for (var i = 0; i < 6; i++)
            {
                ret[i] = _map[i, c];
            }
            return ret;
        }
        private void Log(string msg)
        {
            Console.WriteLine($"深度[{_deth}] {_description}:{msg}");
        }
        private void Log(string msg, ConsoleColor c)
        {
            var orgin = Console.ForegroundColor;

            Console.ForegroundColor = c;
            Log(msg);
            Console.ForegroundColor = orgin;
        }
        public override string ToString()
        {
            return Output;
        }
    }
}
