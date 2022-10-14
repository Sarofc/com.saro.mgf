using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Saro.UI.Tests
{
    public class TestSuperScrollRectController : MonoBehaviour, ISuperScrollRectDataProvider
    {
        public InputField input_count;
        public Button btn_add;

        public InputField input_index;
        public Button btn_jump;

        public Button btn_refresh;

        public List<int> datas = new();
        private SuperScrollRect[] views;

        int ISuperScrollRectDataProvider.GetCellCount()
        {
            return datas.Count;
        }

        void ISuperScrollRectDataProvider.SetCell(GameObject cell, int index)
        {
            cell.GetComponentInChildren<Text>().text = $"index {index} : {datas[index]}";
        }

        // Use this for initialization
        void Start()
        {
            views = GetComponentsInChildren<SuperScrollRect>();
            foreach (var item in views)
            {
                item.DoAwake(this);
            }

            btn_refresh.onClick.AddListener(() =>
            {
                foreach (var item in views)
                {
                    item.ReloadData();
                }
            });

            btn_add.onClick.AddListener(() =>
            {
                if (int.TryParse(input_count.text, out var count))
                {
                    for (int i = 0; i < count; i++)
                    {
                        datas.Add(i);
                    }

                    foreach (var item in views)
                    {
                        item.ReloadData();
                    }
                }
            });

            btn_jump.onClick.AddListener(() =>
            {
                if (int.TryParse(input_index.text, out var index))
                {
                    foreach (var item in views)
                    {
                        item.JumpTo(index);
                    }
                }
            });
        }
    }
}