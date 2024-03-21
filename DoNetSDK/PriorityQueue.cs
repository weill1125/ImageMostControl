using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Boc.Icms.DoNetSDK.Service;

namespace Com.Boc.Icms.DoNetSDK
{

    
    public delegate void VoidDelegate(string sign);

    public class PriorityQueue<T>
    {

        

        public VoidDelegate RemoveDataListDelegate = null;

        /// <summary>
        /// 维护本类唯一实例的锁(不用static，只对同一个实例有锁)
        /// </summary>
        private readonly object Object = new object();


        public class Data
        {
            /// <summary>
            /// 维护本类唯一实例的锁(不用static，只对同一个实例有锁)
            /// </summary>
            private readonly object Object = new object();

            private string sign = "";
            public string Sign
            {
                get
                {
                    lock (Object)
                    {
                        return sign;
                    }
                }
                set
                {
                    lock (Object)
                    {
                        sign = value;
                    }
                }
            }

            public List<T> DataList = new List<T>();

        }

        public Data this[int index] 
        {
            get
            {
                lock (Object)
                {
                    return queueList[index];
                }
            }
        }

        public List<Data> this[int index,int count] 
        {
            get
            {
                lock (Object)
                {
                    return queueList.GetRange(index, count);
                }
            }
        }

        //每个list的容量
        private int capacity = 100;

        private int firstListCapacity = 5;

        //列
        private int currentColumn = 0;
        //行
        private int currentRow = 0;
        public int CurrentIndex
        {
            get
            {
                lock (Object)
                {
                    return currentRow;
                }
            }
        }


        private List<Data> queueList = new List<Data>();

        public PriorityQueue(int firstListCapacity, int capacity)
        {
            this.firstListCapacity = firstListCapacity;
            this.capacity = capacity;
        }

        public PriorityQueue()
        {

            int result = -1;
            int.TryParse(CommonFunc.GetJsonNodeValue("//firstListCapacity"), out result);

            if (result > 0)
            {
                this.firstListCapacity = result;
            }


            int.TryParse(CommonFunc.GetJsonNodeValue("//capacity"), out result);

            if (result > 0)
            {
                this.capacity = result;
            }

        }

        public string CurrentSign
        {
            get
            {
                if (queueList.Count > 0)
                {
                    return queueList[currentRow].Sign;
                }
                else
                {
                    return "";
                }
            }
        }

        
        

        public Data CurrentData
        {
            get
            {
                lock (Object)
                {
                    if (queueList.Count > 0)
                    {
                        return queueList[currentRow];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public int DataCount
        {

            get
            {
                return queueList.Count;
            }
        }



        public void Remove(Predicate<T> match, bool ifChangeCursor)
        {

            lock (Object)
            {
                for (int i = 0; i < queueList.Count; i++)
                {
                    int index = queueList[i].DataList.FindIndex(match);

                    if (index > -1)
                    {

                        RemoveItem(i, index);
                    
                        if (ifChangeCursor)
                        {
                            currentRow = i;
                            currentColumn = index;                            
                            reAdjustCurrentIndex();
                        }
                        break;
                    }

                }
            }
        }

        public void moveTo(Predicate<T> match)
        {
            lock (Object)
            {
                for (int i = 0; i < queueList.Count; i++)
                {
                    int index = queueList[i].DataList.FindIndex(match);                    
                    if (index > -1)
                    {
                     
                        currentRow = i;                        
                        currentColumn = index;

                        reAdjustCurrentIndex();

                        break;
                    }

                }
            }
        }

        public T Pop()
        {
            lock (Object)
            {
                if (queueList.Count > 0)
                {
                    T result = queueList[currentRow].DataList[currentColumn];

                    RemoveItem(currentRow, currentColumn);


                    reAdjustCurrentIndex();

                    return result;

                }
                else
                {
                    return default(T);
                }
            }
        }


        private void RemoveItem(int row,int column)
        {
            queueList[row].DataList.RemoveAt(column);


            if (queueList[row].DataList.Count == 0)
            {
                string sign = queueList[row].Sign;
                queueList.RemoveAt(row);

                if (RemoveDataListDelegate != null)
                {
                    RemoveDataListDelegate.Invoke(sign);
                }
            }
        }

        private void reAdjustCurrentIndex()
        {           
            if (queueList.Count > 0)
            {
                if (currentRow > queueList.Count - 1) //当队列移动到最后一个节点时，队列从头开始
                {
                    currentColumn = currentRow = 0; 

                }
                else
                {
                    //调整currentColumn queueList[currentRow].DataList.Count在添加的时候程序保证大于0
                    if (currentColumn > queueList[currentRow].DataList.Count - 1)
                    {
                        currentColumn = 0;

                        //移向下一个数据链
                        currentRow++;
                        //调整currentRow
                        if (currentRow > queueList.Count - 1)
                        {
                            currentRow = 0;
                        }
                    }
                }
                
            }            
        }


        public void Push(T item,bool createNew=false)
        {
            lock (Object)
            {                
                //队列为空或者队列最后一个list已经满了，则增加新的list
                if (queueList.Count == 0 ||
                    (queueList.Count == 1 && queueList[0].DataList.Count == firstListCapacity) ||
                    queueList[queueList.Count - 1].DataList.Count == capacity|| createNew)
                {
                    Data data = new Data();

                    queueList.Add(data);
                }

                queueList[queueList.Count - 1].DataList.Add(item);
            }
        }

        public void Clear()
        {
            lock (Object)
            {
                queueList.Clear();
            }
        }

        public void AddSign(int startIndex)
        {

        }
        
    }
}
