for /f %%f in ('dir /b *tests.dll') do nunit-console /noshadow /xml:%%f.xml %%f
