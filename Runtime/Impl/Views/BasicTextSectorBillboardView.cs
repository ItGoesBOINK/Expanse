using System;
using UnityEngine.UI;

namespace SupaFabulus.Dev.Expanse.Impl.Views
{
    [Serializable]
    public class BasicTextSectorBillboardView : AbstractSectorBillboardView<Text>
    {

        public override void UpdateView()
        {
            string txt = BillboardText;

            int i;
            int c = _fields.Count;
            Text t;

            for (i = 0; i < c; i++)
            {
                t = _fields[i];
                if(t == null) continue;
                t.text = txt;
            }
        }
    }
}