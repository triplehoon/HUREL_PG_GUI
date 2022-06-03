using HUREL_PG_GUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUREL_PG_GUI.Models
{
    public class EventGenerateTest
    {
        public async Task FunctionTest()
        {
            VM_LineScanning.isStart = true;
            VM_LineScanning._EventTransfer.RaiseEvent(); // 이벤트만 발생시켜줌(레이어가 끝났다는 것을 VM에 알려줌)

            await Task.Run(() => Trace.WriteLine("VM으로 event 전달!"));
        }        
    }
}
