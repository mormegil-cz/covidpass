using System;
using CovidPassQr.Lib;

namespace CovidPassQr.Con
{
    class Program
    {
        static void Main(string[] args)
        {
            var covidPass = CovidPass.ParseFromQr(args[0]);
            Console.WriteLine($"{covidPass.GivenName} {covidPass.FamilyName} ({covidPass.GivenNameTranslit} {covidPass.FamilyNameTranslit}), born {covidPass.DateOfBirth.ToShortDateString()}");
        }
    }
}