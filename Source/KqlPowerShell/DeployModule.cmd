echo -- Deploying module --
mkdir %UserProfile%\Documents\WindowsPowerShell\Modules\KqlPowerShell
copy *.dll %UserProfile%\Documents\WindowsPowerShell\Modules\KqlPowerShell
mkdir %UserProfile%\Documents\WindowsPowerShell\Modules\RealTimeKql
copy RealTimeKql.psm1 %UserProfile%\Documents\WindowsPowerShell\Modules\RealTimeKql