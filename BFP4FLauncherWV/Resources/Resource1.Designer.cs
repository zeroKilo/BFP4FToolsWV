﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BFP4FLauncherWV.Resources {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource1 {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource1() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BFP4FLauncherWV.Resources.Resource1", typeof(Resource1).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 01.07.2018_1227 
        /// ähnelt.
        /// </summary>
        internal static string BuildDate {
            get {
                return ResourceManager.GetString("BuildDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die +webSiteHostName &quot;#IP#&quot; +battleFundsHostName &quot;#IP#&quot; +survey 0 +dc 1 +sessionId #SESSION# +lang en +soldierName &quot;#PLAYER#&quot; +multi 1 +frontendUrl &quot;http://#IP#:1234/&quot; +autoLogin 1 +loggedIn &quot;true&quot; +webBrowser 0 +magmaProtocol http +magmaHost #IP# +punkBuster 0 ähnelt.
        /// </summary>
        internal static string client_startup {
            get {
                return ResourceManager.GetString("client_startup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Ressource vom Typ System.Byte[].
        /// </summary>
        internal static byte[] redi {
            get {
                object obj = ResourceManager.GetObject("redi", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die #IP#    jeff.easylocaldev.com
        ///#IP#    battlefield.play4free.com
        ///#IP#    dev.easy.ea.com
        ///#IP#    dev2.easy.ea.com
        ///#IP#    dev3.easy.ea.com
        ///#IP#    dev4.easy.ea.com
        ///#IP#    playtest1.easy.ea.com
        ///#IP#    playtest1.easy.ea.com
        ///#IP#    private.battlefield.play4free.com
        ///#IP#    preprod.battlefield.play4free.com
        ///#IP#    pte.battlefield.play4free.com
        ///#IP#    gosredirector.ea.com
        ///#IP#    peach.online.ea.com
        ///#IP#    nexus.passport.com ähnelt.
        /// </summary>
        internal static string template_hosts_file {
            get {
                return ResourceManager.GetString("template_hosts_file", resourceCulture);
            }
        }
    }
}
