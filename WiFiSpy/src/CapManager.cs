﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFiSpy.src.Packets;

namespace WiFiSpy.src
{
    public class CapManager
    {
        /// <summary>
        /// Get all stations from the cap files without duplicates
        /// </summary>
        /// <param name="CapFiles"></param>
        /// <returns></returns>
        public static Station[] GetStations(CapFile[] CapFiles)
        {
            SortedList<string, Station> StationMacs = new SortedList<string, Station>();

            foreach (CapFile capFile in CapFiles)
            {
                foreach (Station station in capFile.Stations)
                {
                    if (!String.IsNullOrEmpty(station.SourceMacAddressStr))
                    {
                        if (!StationMacs.ContainsKey(station.SourceMacAddressStr))
                        {
                            StationMacs.Add(station.SourceMacAddressStr, station);
                        }
                        else
                        {
                            Station _station = StationMacs[station.SourceMacAddressStr];

                            //merge the data from this point...

                            //copy the probes from other cap files
                            foreach (ProbePacket probe in station.Probes)
                            {
                                if (_station.DataFrames.FirstOrDefault(o => o.TimeStamp.Ticks == probe.TimeStamp.Ticks) == null)
                                {
                                    _station.AddProbe(probe);
                                }
                            }

                            //copy the data frames from other cap files
                            foreach(DataFrame frame in station.DataFrames)
                            {
                                if(_station.DataFrames.FirstOrDefault(o => o.TimeStamp.Ticks == frame.TimeStamp.Ticks) == null)
                                {
                                    _station.AddDataFrame(frame);
                                }
                            }
                        }
                    }
                }
            }

            return StationMacs.Values.ToArray();
        }

        
        /// <summary>
        /// Grab all the Access Points with the same name in the hope these are all extenders
        /// </summary>
        /// <param name="CapFiles"></param>
        /// <returns></returns>
        public static SortedList<string, List<AccessPoint>> GetPossibleExtenders(CapFile[] CapFiles)
        {
            SortedList<string, List<AccessPoint>> AccessPoints = new SortedList<string, List<AccessPoint>>();

            foreach (CapFile capFile in CapFiles)
            {
                SortedList<string, AccessPoint[]> PossibleExtenders = capFile.PossibleExtenders;

                for(int i = 0; i < PossibleExtenders.Count; i++)
                {
                    if (AccessPoints.ContainsKey(PossibleExtenders.Keys[i]))
                    {
                        for (int j = 0; j < PossibleExtenders.Values[i].Length; j++)
                        {
                            AccessPoint extender = PossibleExtenders.Values[i][j];
                            if (AccessPoints[PossibleExtenders.Keys[i]].FirstOrDefault(o => o.MacAddress == extender.MacAddress) == null)
                            {
                                AccessPoints[PossibleExtenders.Keys[i]].Add(extender);
                            }
                        }
                    }
                    else
                    {
                        AccessPoints.Add(PossibleExtenders.Keys[i], new List<AccessPoint>(PossibleExtenders.Values[i]));
                    }
                }
            }

            return AccessPoints;
        }
    }
}