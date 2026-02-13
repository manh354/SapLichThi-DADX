using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.DynamicPooling
{

    public abstract class _Container<Element,Box>
    {
        /* Luu y, day la class test, khong co chuc nang gi ca, 
         * Chi la class mau de co the build code de hon
         * KHONG CHO VAO DU AN
         */
        public Box box;
        public List<Element> elements;
        public int capacity;
        public int remainignCapacity;
        public _Container(Box box) 
        { 
            this.box = box;
            elements = new List<Element>();
            capacity = InitGetBoxCapacity();
            remainignCapacity = capacity;
        }
        public abstract int InitGetBoxCapacity();
        public abstract int GetRemainingCapacity();
        public abstract int GetElementSize(Element e);
        public abstract bool GetElementCompatibilityWithBox(Element e);
        public virtual bool AddToBox(Element e)
        {
            bool compatibility = GetElementCompatibilityWithBox(e);
            if (!compatibility)
            {
                return false;
            }
            int remainingCapacity = GetRemainingCapacity();
            int elementSize = GetElementSize(e);
            bool elementFit = elementSize <= remainingCapacity;
            if (!elementFit)
            {
                return false;
            }
            elements.Add(e);
            remainignCapacity -= elementSize;
            return true;
        }
        public virtual bool RemoveFromBox(Element e) 
        {
            bool removed = elements.Remove(e);
            if (removed)
            {
                int elementSize = GetElementSize(e);
                remainignCapacity += elementSize;
            }
            return removed;
        }
    }
    public abstract class _GenericPooling<Element, Box>
    {
        int totalCapacity;
        int remainingCapacity;
        int usedCapacity;
        List<_Container<Element, Box>> containers;
        
        public abstract void SizeOf(Element element);
        public virtual bool AddElementToPool(Element element)
        {
            foreach(_Container<Element, Box> container in containers)
            {
                if (!container.AddToBox(element))
                    continue;

                return true;
            }
            return false;
        }
        public virtual bool RemoveElementFromPool(Element element)
        {
            foreach (_Container<Element,Box> container in containers)
            {
                if (!container.RemoveFromBox(element))
                    continue;
                return true;
            }
            return false;
        }
        
    }
}
