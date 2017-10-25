using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace shudu6
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new sudoku();

            /*
            //no7
            s.add(0, "005042");
            s.add(1, "000013");
            s.add(2, "006000");
            s.add(3, "000100");
            s.add(4, "630000");
            s.add(5, "250400");
            */

            /*
            //no8
            s.add(0, "000000");
            s.add(1, "064250");
            s.add(2, "500004");
            s.add(3, "300006");
            s.add(4, "051430");
            s.add(5, "000000");*/


            // no9
            s.add(0, "043000");
            s.add(1, "000500");
            s.add(2, "010600");
            s.add(3, "004020");
            s.add(4, "001000");
            s.add(5, "000130");

            /*
            //no10
            s.add(0, "020006");
            s.add(1, "400000");
            s.add(2, "001600");
            s.add(3, "002400");
            s.add(4, "000005");
            s.add(5, "500060");
            */
            /*
            //no11
            s.add(0, "102000");
            s.add(1, "000020");
            s.add(2, "010360");
            s.add(3, "035010");
            s.add(4, "020000");
            s.add(5, "000402");
            */


            Console.WriteLine(s.output);
            s.solution();
            Console.ReadKey();
        }
    }
    public class sudoku
    {

        public string output
        {
            get
            {
                var ret = "";
                for (int r = 0; r < 6; r++)
                {
                    for (int c = 0; c < 6; c++)
                    {
                        ret += map[r, c] + " ";
                    }
                    ret += "\r\n";
                }
                return ret;
            }
        }

        public bool complete
        {
            get
            {
                for (int r = 0; r < 6; r++)
                {
                    for (int c = 0; c < 6; c++)
                    {
                        if (map[r, c] == 0)
                            return false;
                    }
                }
                return true;
            }
        }

        public int count = 0;
        public int sovercount = 0;
        public int[,] map = new int[6, 6];

        public string durtion = "原始";

        public void solution()
        {

            for (int r = 0; r < 6; r++)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (findsolution)
                    {
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId+"已经找到解,退出子线程");
                        return;
                    }
                    Cross(r, i);
                }
            }

            sovercount++;

            if (complete)
            {
                findsolution = true;
                //找到解
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} 找到解:\r\n" + output);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static bool findsolution = false;
        public void add(int r, string s)
        {
            int[] nums = splitnum(s);
            for (int i = 0; i < 6; i++)
            {
                map[r, i] = nums[i];
            }
        }

        public override string ToString()
        {
            return output;
        }

        public int[] splitnum(string s)
        {
            if (s.Length != 6) new Exception("应该是6个数字");
            int[] ret = new int[6];


            var count = 0;
            foreach (var mumstr in s)
            {
                ret[count] = int.Parse(mumstr.ToString());
                count++;
            }
            return ret;
        }
        public void Cross(int r, int c)
        {
            //计算总共是不是有5个数字
            if (map[r, c] != 0)
                return;
            var rowmissnums = getRowMiss(r);
            var colnums = getCol(c);
            var box = getBox(r, c);

            foreach (var i in colnums)
            {
                rowmissnums.Remove(i);
            }

            foreach (var i in box)
            {
                rowmissnums.Remove(i);
            }

            if (rowmissnums.Count == 1)
            {
                //惟一解,写上map
                map[r, c] = rowmissnums[0];
                return;
            }
            if (rowmissnums.Count == 0)
            {
                //无解
                Console.WriteLine($"{count} ,[{r},{c}] 出现0解");
                Thread.CurrentThread.Abort();
                return;
            }

            if (rowmissnums.Count == 2)
            {
                var ss = "";
                foreach (var rstring in rowmissnums)
                {
                    ss += rstring + " ";
                }
                Console.WriteLine($"{count}的[{r},{c}]出现{rowmissnums.Count}个解[ {ss}],分裂");
                foreach (var num in rowmissnums)
                {
                    
                    lock (locker)
                    {
                        //分出两个版本 尝试进行解
                        var one = new sudoku
                        {
                            count = count++,
                            map = (int[,])map.Clone()
                        };
                        one.map[r, c] = num;
                        one.durtion = $"[{r},{c}] 处 [{num}]可能性";

                        var t = new Thread(one.solution);
                        t.Start();
                    }
                }
            }


        }
        static object locker=new object();
        public List<int> getBox(int r, int c)
        {
            var box = new List<int>();
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
            for (int i = 0; i < 3; i++)
            {
                box.Add(map[r, startCol + i]);
                box.Add(map[nextrow, startCol + i]);
            }
            return box;
        }

        public List<int> getRowMiss(int r)
        {
            var rows = getRow(r);
            var miss = new List<int>();
            for (int i = 1; i < 7; i++)
            {
                if (!rows.Contains(i)) miss.Add(i);
            }
            return miss;
        }

        public List<int> getRow(int r)
        {
            var ret = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                ret.Add(map[r, i]);
            }
            return ret;
        }

        public List<int> getCol(int c)
        {
            var ret = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                ret.Add(map[i, c]);
            }
            return ret;
        }

    }

    public class box
    {
        int[,] map = new int[2, 3];
    }

    public class cell
    {


    }


    public class col
    {
        int[] nums = new int[6];
        public List<int> have = new List<int>();

    }

    public class row
    {
        int[] nums = new int[6];
    }



}
