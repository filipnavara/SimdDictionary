﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using SimdDictionary;

namespace Benchmarks {
    public class Program {
        public static void Main (string[] args) {
            var rng = new Random(1234);
            int c = 4096, d = 4096 * 5;
            List<long> keys = new (c),
                values = new (c);
            var test = new SimdDictionary<long, long>(c);

            for (int i = 0; i < c; i++) {
                keys.Add(rng.NextInt64());
                values.Add(i * 2 + 1);
            }

            for (int i = 0; i < c; i++)
                test.Add(keys[i], values[i]);

            // Integrity check
            var expectedKeySet = keys.OrderBy(k => k).ToList();
            var enumeratedKeySet = test.Keys.OrderBy(k => k).ToList();
            if (!expectedKeySet.SequenceEqual(enumeratedKeySet)) {
                var missing = new HashSet<long>(keys);
                missing.ExceptWith(enumeratedKeySet);
                throw new KeyNotFoundException();
            }

            for (int j = 0; j < d; j++)
                for (int i = 0; i < c; i++)
                    if (!test.TryGetValue(keys[i], out _))
                        throw new Exception();

            var keyList = test.Keys.ToArray();
            var valueList = test.Values.ToArray();

            var copy = new SimdDictionary<long, long>(test);
            for (int i = 0; i < c; i++)
                if (!copy.TryGetValue(keys[i], out _))
                    throw new Exception();

            for (int i = 0; i < c; i++)
                if (!test.Remove(keys[i]))
                    throw new Exception();

            for (int i = 0; i < c; i++)
                if (test.TryGetValue(keys[i], out _))
                    throw new Exception();

            if (test.Count != 0)
                throw new Exception();

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                .Run(args, GetConfig());

            Console.ReadLine();
        }

        public static IConfig GetConfig () =>
            DefaultConfig.Instance
                .WithOption(ConfigOptions.JoinSummary, true);
    }
}
