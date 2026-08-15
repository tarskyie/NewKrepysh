using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NewKrepysh.WinUI.Models;

namespace NewKrepysh.WinUI.Services
{
    public static class SiteBuilder
    {
        public static void Build(IList<SitePage> pages, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            // 1. Assign unique safe filenames to all pages in the tree
            var pageToFilename = new Dictionary<SitePage, string>();
            AssignFilenames(pages, pageToFilename);

            // 2. Write the stylesheet style.css
            string cssPath = Path.Combine(outputDirectory, "style.css");
            File.WriteAllText(cssPath, GetCssStyles());

            // If no pages exist, create a default index.html
            if (!pages.Any())
            {
                string defaultHtml = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Empty Site</title>
    <link rel=""stylesheet"" href=""style.css"" />
</head>
<body>
    <main class=""content"">
        <h1 class=""page-title"">Welcome</h1>
        <div class=""markup-content"">
            <p>This static site is empty. Add pages in New Krepysh to populate it.</p>
        </div>
    </main>
</body>
</html>";
                File.WriteAllText(Path.Combine(outputDirectory, "index.html"), defaultHtml);
                return;
            }

            // 3. Generate HTML files for each page
            void BuildPageRecursive(SitePage page)
            {
                string filename = pageToFilename[page];
                string filePath = Path.Combine(outputDirectory, filename);

                string navigationHtml = GenerateNavigationHtml(pages, pageToFilename, page);

                string pageHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{HttpUtility.HtmlEncode(page.Title)}</title>
    <link rel=""stylesheet"" href=""style.css"" />
</head>
<body>
    
    <div class=""sidebar-backdrop""></div>
    <nav class=""sidebar"">
        <h2>Navigation</h2>
        {navigationHtml}
    </nav>
    <main class=""content"">
<div style=""display: flex; flex-direction: row;"">
        <button class=""nav-toggle"" aria-label=""Toggle navigation"" aria-expanded=""false"">☰</button>
        <h1 class=""page-title"">{HttpUtility.HtmlEncode(page.Title)}</h1>
</div>
        <div class=""markup-content"">
            {page.HtmlContent}
        </div>
    </main>
    <script>
        const navToggle = document.querySelector('.nav-toggle');
        const body = document.body;
        const backdrop = document.querySelector('.sidebar-backdrop');" + @"
        
        function toggleNav() {
            const isOpen = body.classList.toggle('nav-open');
            navToggle.setAttribute('aria-expanded', isOpen);
        }
        
        navToggle.addEventListener('click', toggleNav);
        backdrop.addEventListener('click', toggleNav);
    </script>
</body>
</html>";
                File.WriteAllText(filePath, pageHtml);

                foreach (var child in page.Children)
                {
                    BuildPageRecursive(child);
                }
            }

            foreach (var page in pages)
            {
                BuildPageRecursive(page);
            }
        }

        private static void AssignFilenames(IList<SitePage> pages, Dictionary<SitePage, string> pageToFilename)
        {
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // First pass: identify "Home" or the first root page and set as index.html
            SitePage? homePage = pages.FirstOrDefault(p => p.Title.Equals("Home", StringComparison.OrdinalIgnoreCase))
                                 ?? pages.FirstOrDefault();

            if (homePage != null)
            {
                pageToFilename[homePage] = "index.html";
                usedNames.Add("index.html");
            }

            // Helper to recursively assign names
            void AssignRecursive(SitePage page)
            {
                if (!pageToFilename.ContainsKey(page))
                {
                    string sanitized = SanitizeFilename(page.Title);
                    if (string.IsNullOrWhiteSpace(sanitized)) sanitized = "page";

                    string candidate = sanitized + ".html";
                    int counter = 1;
                    while (usedNames.Contains(candidate))
                    {
                        candidate = $"{sanitized}-{counter}.html";
                        counter++;
                    }
                    pageToFilename[page] = candidate;
                    usedNames.Add(candidate);
                }

                foreach (var child in page.Children)
                {
                    AssignRecursive(child);
                }
            }

            foreach (var page in pages)
            {
                AssignRecursive(page);
            }
        }

        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return "page";

            // Remove invalid filename characters, convert spaces/special chars to hyphens
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]|\s)+", invalidChars);
            string sanitized = Regex.Replace(filename, invalidRegStr, "-");

            sanitized = sanitized.Trim('-').ToLowerInvariant();
            return string.IsNullOrEmpty(sanitized) ? "page" : sanitized;
        }

        private static string GenerateNavigationHtml(IList<SitePage> pages, Dictionary<SitePage, string> pageToFilename, SitePage currentPage)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<ul class=\"nav-list\">");
            foreach (var page in pages)
            {
                RenderNavigationItem(page, pageToFilename, currentPage, sb, 0);
            }
            sb.AppendLine("</ul>");
            return sb.ToString();
        }

        private static void RenderNavigationItem(SitePage page, Dictionary<SitePage, string> pageToFilename, SitePage currentPage, StringBuilder sb, int depth)
        {
            string filename = pageToFilename.TryGetValue(page, out string? name) ? name : "index.html";
            string activeClass = page == currentPage ? "active" : "";
            string indent = new string(' ', depth * 4);

            sb.AppendLine($"{indent}<li class=\"nav-item depth-{depth} {activeClass}\">");
            sb.AppendLine($"{indent}    <a href=\"{filename}\" class=\"nav-link {activeClass}\">{HttpUtility.HtmlEncode(page.Title)}</a>");

            if (page.Children != null && page.Children.Any())
            {
                sb.AppendLine($"{indent}    <ul class=\"nav-sublist\">");
                foreach (var child in page.Children)
                {
                    RenderNavigationItem(child, pageToFilename, currentPage, sb, depth + 1);
                }
                sb.AppendLine($"{indent}    </ul>");
            }
            sb.AppendLine($"{indent}</li>");
        }

        private static string GetCssStyles()
        {
            return @":root {
    --primary-color: #0078d4;
    --bg-color: #f3f2f1;
    --sidebar-bg: #faf9f8;
    --text-color: #323130;
    --border-color: #edebe9;
    --hover-color: #eaeaea;
    --active-bg: #edebe9;
    --sidebar-width: 280px;
}

* {
    box-sizing: border-box;
}

body {
    margin: 0;
    font-family: ""Segoe UI"", -apple-system, BlinkMacSystemFont, Roboto, ""Helvetica Neue"", Arial, sans-serif;
    color: var(--text-color);
    background-color: #ffffff;
    display: flex;
    min-height: 100vh;
    line-height: 1.5;
}

.sidebar {
    width: var(--sidebar-width);
    background-color: var(--sidebar-bg);
    border-right: 1px solid var(--border-color);
    padding: 20px;
    flex-shrink: 0;
    overflow-y: auto;
    position: sticky;
    top: 0;
    height: 100vh;
    transition: transform 0.3s ease-in-out;
    z-index: 1000;
}

.sidebar h2 {
    font-size: 1.2rem;
    margin-top: 0;
    margin-bottom: 20px;
    color: var(--primary-color);
}

.content {
    flex-grow: 1;
    padding: 10px;
    overflow-y: auto;
    max-width: 100%;
}

.nav-list,
.nav-sublist {
    list-style: none;
    padding-left: 0;
    margin: 0;
}

.nav-sublist {
    padding-left: 16px;
    border-left: 1px solid var(--border-color);
    margin-top: 4px;
    margin-bottom: 4px;
}

.nav-item {
    margin-bottom: 4px;
}

.nav-link {
    display: block;
    padding: 8px 10px;
    text-decoration: none;
    color: var(--text-color);
    border-radius: 4px;
    font-size: 0.95rem;
    transition: background-color 0.15s ease;
    word-break: break-word;
}

.nav-link:hover {
    background-color: var(--hover-color);
}

.nav-link.active {
    font-weight: 600;
    color: var(--primary-color);
    background-color: var(--active-bg);
}

.page-title {
    margin-top: 0;
    margin-bottom: 24px;
    font-size: 2rem;
    border-bottom: 1px solid var(--border-color);
    padding-bottom: 12px;
    word-break: break-word;
}

.nav-toggle {
    display: none;
    top: 16px;
    left: 16px;
    background-color: var(--primary-color);
    color: #ffffff;
    border: none;
    border-radius: 4px;
    padding: 10px 12px;
    font-size: 1.2rem;
    cursor: pointer;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
    transition: background-color 0.15s ease;
}

.nav-toggle:hover {
    background-color: #106ebe;
}

.sidebar-backdrop {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.4);
    z-index: 999;
    opacity: 0;
    transition: opacity 0.3s ease-in-out;
}

@media (max-width: 768px) {
    .sidebar {
        position: fixed;
        top: 0;
        left: 0;
        transform: translateX(-100%);
        height: 100vh;
        box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
    }
    
    .nav-open .sidebar {
        transform: translateX(0);
    }
    
    .nav-open .sidebar-backdrop {
        display: block;
        opacity: 1;
    }
    
    .nav-toggle {
        display: block;
    }
    
    .content {
        padding: 10px;
        width: 100%;
    }
    
    .page-title {
        font-size: 1.6rem;
        margin-bottom: 20px;
    }
}
";
        }
    }
}