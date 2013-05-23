using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

class Programa{
	private static AnalizadorLexico al;
	public static void Main(string[] args){
		if (args.Length > 0) {
			al = new AnalizadorLexico (args[0]);
			AnalizadorLexico.Token tk = new AnalizadorLexico.Token ();
			tk = al.analizador ();
			while( tk.tt != AnalizadorLexico.TipoToken.TK_EOF ){
				if( !tk.lexema.Equals("")){
					Console.WriteLine("( "+tk.tt+", "+tk.lexema+" )");
					tk.lexema = "";
				}
				tk = al.analizador ();
			}
		} else {
			Console.WriteLine ("Error");
		}
	}
}
