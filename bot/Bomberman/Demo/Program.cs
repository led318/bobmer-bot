/*-
 * #%L
 * Codenjoy - it's a dojo-like platform from developers to developers.
 * %%
 * Copyright (C) 2018 Codenjoy
 * %%
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public
 * License along with this program.  If not, see
 * <http://www.gnu.org/licenses/gpl-3.0.html>.
 * #L%
 */
using System;
using System.Threading.Tasks;
using System.Configuration;
using Bomberman.Api.Infrastructure;

namespace Demo
{
    class Program
    {

        // you can get this code after registration on the server with your email
        //static string ServerUrl = "https://botchallenge.cloud.epam.com/codenjoy-contest/board/player/y2xvmwbn1tkpur93x38n?code=1938052386392496631";
        //static string ServerUrl = "http://127.0.0.1:8080/codenjoy-contest/board/player/y2xvmwbn1tkpur93x38n?code=1938052386392496631";

        static void Main(string[] args)
        {
            //var isLocal = true;
            //var isFile = false;

            //var serverUrl = ConfigurationSettings.AppSettings["connectionString"];
            var serverUrl = Config.ConnectionString;

            if (Config.IsLocal)
            {
                if (Config.PrintBoard)
                    Console.SetWindowSize(Console.LargestWindowWidth - 50, Console.LargestWindowHeight - 3);

                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;

                serverUrl = Config.ConnectionStringLocal;
            }

            // creating custom AI client
            var bot = new YourSolver(serverUrl, Config.IsFile);

            // starting thread with playing game
            //Task.Run(bot.Play);
            bot.Play();

            // waiting for any key
            ReadKey();

            // on any key - asking AI client to stop.
            bot.InitiateExit();
        }

        private static void ReadKey() {
            while (true)
            {
                // waiting for any key
                var key = Console.ReadKey();

                Global.ManualMove = key.KeyChar.ToString();
            }
        }
    }
}
