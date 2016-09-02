﻿using System;
using System.Linq;
using mRemoteNG.App;
using mRemoteNG.Connection;
using mRemoteNG.Container;
using mRemoteNG.Security;
using mRemoteNG.Tree;
using mRemoteNG.Tree.Root;


namespace mRemoteNG.Config.Connections
{
    public class CsvConnectionsSerializerMremotengFormat : ISerializer
    {
        private string _csv = "";

        public Save SaveSecurity { get; set; }

        public string Serialize(ConnectionTreeModel connectionTreeModel)
        {
            var rootNode = (RootNodeInfo)connectionTreeModel.RootNodes.First(node => node is RootNodeInfo);
            return SerializeToCsv(rootNode);
        }

        private string SerializeToCsv(RootNodeInfo rootNodeInfo)
        {
            if (Runtime.IsConnectionsFileLoaded == false)
                return "";
            _csv = "";
            WriteCsvHeader();
            SerializeNodesRecursive(rootNodeInfo);
            return _csv;
        }

        private void WriteCsvHeader()
        {
            var csvHeader = string.Empty;
            csvHeader += "Name;Folder;Description;Icon;Panel;";
            if (SaveSecurity.Username)
                csvHeader += "Username;";
            if (SaveSecurity.Password)
                csvHeader += "Password;";
            if (SaveSecurity.Domain)
                csvHeader += "Domain;";
            csvHeader += "Hostname;Protocol;PuttySession;Port;ConnectToConsole;UseCredSsp;RenderingEngine;ICAEncryptionStrength;RDPAuthenticationLevel;LoadBalanceInfo;Colors;Resolution;AutomaticResize;DisplayWallpaper;DisplayThemes;EnableFontSmoothing;EnableDesktopComposition;CacheBitmaps;RedirectDiskDrives;RedirectPorts;RedirectPrinters;RedirectSmartCards;RedirectSound;RedirectKeys;PreExtApp;PostExtApp;MacAddress;UserField;ExtApp;VNCCompression;VNCEncoding;VNCAuthMode;VNCProxyType;VNCProxyIP;VNCProxyPort;VNCProxyUsername;VNCProxyPassword;VNCColors;VNCSmartSizeMode;VNCViewOnly;RDGatewayUsageMethod;RDGatewayHostname;RDGatewayUseConnectionCredentials;RDGatewayUsername;RDGatewayPassword;RDGatewayDomain;";
            if (SaveSecurity.Inheritance)
                csvHeader += "InheritCacheBitmaps;InheritColors;InheritDescription;InheritDisplayThemes;InheritDisplayWallpaper;InheritEnableFontSmoothing;InheritEnableDesktopComposition;InheritDomain;InheritIcon;InheritPanel;InheritPassword;InheritPort;InheritProtocol;InheritPuttySession;InheritRedirectDiskDrives;InheritRedirectKeys;InheritRedirectPorts;InheritRedirectPrinters;InheritRedirectSmartCards;InheritRedirectSound;InheritResolution;InheritAutomaticResize;InheritUseConsoleSession;InheritUseCredSsp;InheritRenderingEngine;InheritUsername;InheritICAEncryptionStrength;InheritRDPAuthenticationLevel;InheritLoadBalanceInfo;InheritPreExtApp;InheritPostExtApp;InheritMacAddress;InheritUserField;InheritExtApp;InheritVNCCompression;InheritVNCEncoding;InheritVNCAuthMode;InheritVNCProxyType;InheritVNCProxyIP;InheritVNCProxyPort;InheritVNCProxyUsername;InheritVNCProxyPassword;InheritVNCColors;InheritVNCSmartSizeMode;InheritVNCViewOnly;InheritRDGatewayUsageMethod;InheritRDGatewayHostname;InheritRDGatewayUseConnectionCredentials;InheritRDGatewayUsername;InheritRDGatewayPassword;InheritRDGatewayDomain";
            _csv += csvHeader;
        }

        private void SerializeNodesRecursive(ContainerInfo containerInfo)
        {
            foreach (var child in containerInfo.Children)
            {
                if (child is ContainerInfo)
                    SerializeNodesRecursive((ContainerInfo)child);
                else
                    SerializeConnectionInfo(child);
            }
        }

        private void SerializeConnectionInfo(ConnectionInfo con)
        {
            var nodePath = con.TreeNode.FullPath;

            var firstSlash = nodePath.IndexOf("\\", StringComparison.Ordinal);
            nodePath = nodePath.Remove(0, firstSlash + 1);
            var lastSlash = nodePath.LastIndexOf("\\", StringComparison.Ordinal);

            nodePath = lastSlash > 0 ? nodePath.Remove(lastSlash) : "";

            var csvLine = Environment.NewLine;

            csvLine += con.Name + ";" + nodePath + ";" + con.Description + ";" + con.Icon + ";" + con.Panel + ";";

            if (SaveSecurity.Username)
                csvLine += con.Username + ";";

            if (SaveSecurity.Password)
                csvLine += con.Password + ";";

            if (SaveSecurity.Domain)
                csvLine += con.Domain + ";";

            csvLine += con.Hostname + ";" + con.Protocol + ";" + con.PuttySession + ";" + Convert.ToString(con.Port) + ";" + Convert.ToString(con.UseConsoleSession) + ";" + Convert.ToString(con.UseCredSsp) + ";" + con.RenderingEngine + ";" + con.ICAEncryptionStrength + ";" + con.RDPAuthenticationLevel + ";" + con.LoadBalanceInfo + ";" + con.Colors + ";" + con.Resolution + ";" + Convert.ToString(con.AutomaticResize) + ";" + Convert.ToString(con.DisplayWallpaper) + ";" + Convert.ToString(con.DisplayThemes) + ";" + Convert.ToString(con.EnableFontSmoothing) + ";" + Convert.ToString(con.EnableDesktopComposition) + ";" + Convert.ToString(con.CacheBitmaps) + ";" + Convert.ToString(con.RedirectDiskDrives) + ";" + Convert.ToString(con.RedirectPorts) + ";" + Convert.ToString(con.RedirectPrinters) + ";" + Convert.ToString(con.RedirectSmartCards) + ";" + con.RedirectSound + ";" + Convert.ToString(con.RedirectKeys) + ";" + con.PreExtApp + ";" + con.PostExtApp + ";" + con.MacAddress + ";" + con.UserField + ";" + con.ExtApp + ";" + con.VNCCompression + ";" + con.VNCEncoding + ";" + con.VNCAuthMode + ";" + con.VNCProxyType + ";" + con.VNCProxyIP + ";" + Convert.ToString(con.VNCProxyPort) + ";" + con.VNCProxyUsername + ";" + con.VNCProxyPassword + ";" + con.VNCColors + ";" + con.VNCSmartSizeMode + ";" + Convert.ToString(con.VNCViewOnly) + ";";

            if (SaveSecurity.Inheritance)
            {
                csvLine += con.Inheritance.CacheBitmaps + ";" + 
                    con.Inheritance.Colors + ";" +
                    con.Inheritance.Description + ";" +
                    con.Inheritance.DisplayThemes + ";" +
                    con.Inheritance.DisplayWallpaper + ";" +
                    con.Inheritance.EnableFontSmoothing + ";" +
                    con.Inheritance.EnableDesktopComposition + ";" +
                    con.Inheritance.Domain + ";" +
                    con.Inheritance.Icon + ";" +
                    con.Inheritance.Panel + ";" +
                    con.Inheritance.Password + ";" +
                    con.Inheritance.Port + ";" +
                    con.Inheritance.Protocol + ";" +
                    con.Inheritance.PuttySession + ";" +
                    con.Inheritance.RedirectDiskDrives + ";" +
                    con.Inheritance.RedirectKeys + ";" +
                    con.Inheritance.RedirectPorts + ";" +
                    con.Inheritance.RedirectPrinters + ";" +
                    con.Inheritance.RedirectSmartCards + ";" +
                    con.Inheritance.RedirectSound + ";" +
                    con.Inheritance.Resolution + ";" +
                    con.Inheritance.AutomaticResize + ";" +
                    con.Inheritance.UseConsoleSession + ";" +
                    con.Inheritance.UseCredSsp + ";" +
                    con.Inheritance.RenderingEngine + ";" +
                    con.Inheritance.Username + ";" +
                    con.Inheritance.ICAEncryptionStrength + ";" +
                    con.Inheritance.RDPAuthenticationLevel + ";" +
                    con.Inheritance.LoadBalanceInfo + ";" +
                    con.Inheritance.PreExtApp + ";" +
                    con.Inheritance.PostExtApp + ";" +
                    con.Inheritance.MacAddress + ";" +
                    con.Inheritance.UserField + ";" +
                    con.Inheritance.ExtApp + ";" +
                    con.Inheritance.VNCCompression + ";" +
                    con.Inheritance.VNCEncoding + ";" +
                    con.Inheritance.VNCAuthMode + ";" +
                    con.Inheritance.VNCProxyType + ";" +
                    con.Inheritance.VNCProxyIP + ";" +
                    con.Inheritance.VNCProxyPort + ";" +
                    con.Inheritance.VNCProxyUsername + ";" +
                    con.Inheritance.VNCProxyPassword + ";" +
                    con.Inheritance.VNCColors + ";" +
                    con.Inheritance.VNCSmartSizeMode + ";" +
                    con.Inheritance.VNCViewOnly;
            }

            _csv += csvLine;
        }
    }
}