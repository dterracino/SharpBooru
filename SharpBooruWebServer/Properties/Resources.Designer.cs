﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.17929
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TA.SharpBooru.Client.WebServer.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TA.SharpBooru.Client.WebServer.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Sucht eine lokalisierte Ressource vom Typ System.Byte[].
        /// </summary>
        internal static byte[] favicon_ico {
            get {
                object obj = ResourceManager.GetObject("favicon_ico", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;script type=&quot;text/javascript&quot;&gt;
        ///    window.onload = function () {
        ///		mimg = document.getElementById(&quot;mimg&quot;);
        ///        mimg.ondblclick = function () {
        ///			if (mimg.style.maxWidth == &quot;none&quot;) {
        ///				mimg.style.maxWidth = &quot;800px&quot;;
        ///				mimg.style.maxHeight = &quot;800px&quot;;
        ///			} else {
        ///				mimg.style.maxWidth = &quot;none&quot;;
        ///				mimg.style.maxHeight = &quot;none&quot;;
        ///			}
        ///        }
        ///    }
        ///&lt;/script&gt; ähnelt.
        /// </summary>
        internal static string imgscript_js {
            get {
                return ResourceManager.GetString("imgscript_js", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die body  {{
        ///    background-color: #2c2c2c;
        ///    margin: 0px;
        ///}}
        ///
        ///a {{
        ///    text-decoration: none;
        ///    color: inherit;
        ///}}
        ///
        ///div.main {{
        ///    color: white;
        ///    font: 16px arial;
        ///    max-width: 100%;
        ///}}
        ///
        ///div.header {{
        ///    position: fixed;
        ///    left: 0px;
        ///    top: 0px;
        ///    width: 100%;
        ///    height: 88px;
        ///    white-space: nowrap;
        ///    overflow: hidden;
        ///    box-shadow: 0px 0px 16px black;
        ///    background-color: black;
        ///    background-image: -moz-linear-gradient(
        ///        45deg, 
        ///        rgba(76,76, [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        internal static string style_css {
            get {
                return ResourceManager.GetString("style_css", resourceCulture);
            }
        }
    }
}
