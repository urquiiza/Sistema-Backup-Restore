using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tela;
using Tela.ViewModels;

namespace Tela.ViewModels
{
    public class MainViewModel
    {
        private string _origemBanco = string.Empty;  
        public string OrigemBanco { get => _origemBanco.Trim(); set => _origemBanco = value; }
        
        private string _destinoBanco = string.Empty;
        public string DestinoBanco { get => _destinoBanco; set => _destinoBanco = value; }

        private bool _selecaoFire25;
        public bool SelecaoFire25 { get => _selecaoFire25; set => _selecaoFire25 = value; }
        
        private bool _selecaoFire40 = true;
        public bool SelecaoFire40 { get => _selecaoFire40; set => _selecaoFire40 = value; }
    }
}