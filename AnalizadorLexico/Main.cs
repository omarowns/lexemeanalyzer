using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

class Programa{
	private static AnalizadorLexico al;
	public static void Main(string[] args){
		if (args.Length > 0) {
			al = new AnalizadorLexico (args[0]);
			al.lectura ();
		} else {
			Console.WriteLine ("Error");
		}
	}
}
