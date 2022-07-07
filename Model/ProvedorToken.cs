﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixNET.Model
{
    public enum ProvedorToken
    {
        NONE = 0,
        SICOOB = 756,
        Santander = 33,
        BancoBrasil = 1,
        Itau = 341,
        Bradesco = 237
    }

    public enum TipoApi
    {
        Boleto = 0,
        Pix = 1
    }
}
