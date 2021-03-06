﻿using EFCore_DI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace EFCore_DI
{
  class Program
  {
    const int sifraArtikla = 12345678;
    private static IServiceProvider serviceProvider;
    public static void Main(string[] args)
    {
      //Inicijaliziraj postavke za DI
      serviceProvider = new ServiceCollection()
                            .AddSingleton<IConnectionStringTool, ConnectionStringTool>()
                            .AddTransient<FirmaContext, FirmaContext>()
                            .BuildServiceProvider();

      DodajArtikl();
      IzmijeniCijenuArtikla();
      ObrisiArtikl();
      DohvatiNajskuplje_v2();
    }

    private static void DodajArtikl()
    {
      try
      {
        using (var context = serviceProvider.GetService<FirmaContext>())
        {
          //primjer dodavanja (1)
          Artikl artikl = new Artikl()
          {
            SifArtikla = sifraArtikla,
            CijArtikla = 10m,
            JedMjere = "kom",
            NazArtikla = "Burek sa sirom"
          };
          context.Artikl.Add(artikl);
          context.Set<Artikl>().Add(artikl);
          context.SaveChanges();
          Console.WriteLine($"Artikl broj {artikl.SifArtikla} pohranjen u bazu podataka");
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine("Pogreška prilikom pohrane artikla broj {0} u bazu podataka : {1}", sifraArtikla, exc.CompleteExceptionMessage());
      }
    }

    private static void IzmijeniCijenuArtikla()
    {
      try
      {
        using (var context = serviceProvider.GetService<FirmaContext>())
        {
          Artikl artikl = context.Artikl.Find(sifraArtikla);
          artikl.CijArtikla = 11m;
          context.SaveChanges();
          Console.WriteLine("Cijena artikla izmijenjena");
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine("Pogreška prilikom izmjene cijene artikla {0} u bazu podataka : {1}", sifraArtikla, exc.CompleteExceptionMessage());
      }
    }

    private static void ObrisiArtikl()
    {
      try
      {
        using (var context = serviceProvider.GetService<FirmaContext>())
        {
          //Artikl artikl = context.Find<Artikl>(sifraArtikla);
          Artikl artikl = context.Artikl.Find(sifraArtikla);
          context.Artikl.Remove(artikl);
          //context.Entry(artikl).State = EntityState.Deleted;
          context.SaveChanges();
          Console.WriteLine("Artikl obrisan");
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine("Pogreška prilikom brisanje artikla broj {0} : {1}", sifraArtikla, exc.CompleteExceptionMessage());
      }
    }

    private static void DohvatiNajskuplje()
    {
      Console.WriteLine("Unesite cijeli broj za prvih N najskupljih artikala");
      int n = int.Parse(Console.ReadLine());
      using (var context = serviceProvider.GetService<FirmaContext>())
      {
        var upit = context.Artikl
                          .Include(a => a.Stavka)
                          .AsNoTracking()
                          .OrderByDescending(a => a.CijArtikla)
                          .Take(n);
        int cnt = 0;
        foreach (Artikl artikl in upit)
        {
          Console.WriteLine("{0}. {1} - {2:C2} ({3})", ++cnt, artikl.NazArtikla, artikl.CijArtikla, artikl.Stavka.Count);
        }
      }
    }

    private static void DohvatiNajskuplje_v2()
    {
      Console.WriteLine("Unesite cijeli broj za prvih N najskupljih artikala");
      int n = int.Parse(Console.ReadLine());
      using (var context = serviceProvider.GetService<FirmaContext>())
      {
        var upit = context.Artikl
                          .OrderByDescending(a => a.CijArtikla)
                          .Select(a => new { a.NazArtikla, a.CijArtikla, BrojProdanih = a.Stavka.Count })
                          .Take(n);

        int cnt = 0;
        foreach (var artikl in upit)
        {
          Console.WriteLine("{0}. {1} - {2:C2} ({3})", ++cnt, artikl.NazArtikla, artikl.CijArtikla, artikl.BrojProdanih);
        }
      }
    }
  }
}