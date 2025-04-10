namespace WPF_App.Services
{
    public class OpcNodeConfiguration
    {
        public string Name { get; set; }
        public string NodeId { get; set; }
        public Type DataType { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class OpcConfiguration
    {
        public List<OpcNodeConfiguration> Nodes { get; } = new List<OpcNodeConfiguration>();

        public OpcNodeConfiguration GetNode(string name)
        {
            return Nodes.Find(n => n.Name == name);
        }

        public OpcConfiguration AddNode(string name, string nodeId, Type dataType, bool isReadOnly = false)
        {
            Nodes.Add(new OpcNodeConfiguration
            {
                Name = name,
                NodeId = nodeId,
                DataType = dataType,
                IsReadOnly = isReadOnly
            });
            return this;
        }
    }

    public class RobinLineOpcConfiguration : OpcConfiguration
    {
        public RobinLineOpcConfiguration()
        {
            // Robots
            AddNode("Robot1Inclusion", "ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_robot_robin_primer", typeof(bool));
            AddNode("Robot2Inclusion", "ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_robot_robin_colla", typeof(bool));
            AddNode("Robot1Ready", "ns=2;s=Tags.Eren_robin/pc_robot_robin_primer_ready", typeof(bool), true);
            AddNode("Robot2Ready", "ns=2;s=Tags.Eren_robin/pc_robot_robin_colla_ready", typeof(bool), true);

            // Oven 1
            AddNode("Oven1Inclusion", "ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_forno_primer", typeof(bool));
            AddNode("Oven1Ready", "ns=2;s=Tags.Eren_robin/pc_forno_primer_ready", typeof(bool), true);
            AddNode("Oven1Temperature", "ns=2;s=Tags.Eren_robin/pc_temperatura_attuale_forno_primer", typeof(int), true);
            AddNode("Oven1TemperatureReached", "ns=2;s=Tags.Eren_robin/pc_forno_primer_in_temperatura", typeof(bool), true);
            AddNode("Oven1Mode", "ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_primer", typeof(bool), true);
            AddNode("Oven1LampsPercentage", "ns=2;s=Tags.Eren_robin/pc_percentuale_accensione_lampade_forno_primer", typeof(int));
            AddNode("Oven1FanPercentage", "ns=2;s=Tags.Eren_robin/pc_percentuale_velocita_ventole_forno_primer", typeof(int));
            AddNode("Oven1TempSetpoint", "ns=2;s=Tags.Eren_robin/pc_temperatura_massima_forno_primer", typeof(int));

            // Oven 2
            AddNode("Oven2Inclusion", "ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_forno_colla", typeof(bool));
            AddNode("Oven2Ready", "ns=2;s=Tags.Eren_robin/pc_forno_colla_ready", typeof(bool), true);
            AddNode("Oven2Temperature", "ns=2;s=Tags.Eren_robin/pc_temperatura_attuale_forno_colla", typeof(int), true);
            AddNode("Oven2TemperatureReached", "ns=2;s=Tags.Eren_robin/pc_forno_colla_in_temperatura", typeof(bool), true);
            AddNode("Oven2Mode", "ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_colla", typeof(bool), true);
            AddNode("Oven2LampsPercentage", "ns=2;s=Tags.Eren_robin/pc_percentuale_accensione_lampade_forno_colla", typeof(int));
            AddNode("Oven2FanPercentage", "ns=2;s=Tags.Eren_robin/pc_percentuale_velocita_ventole_forno_colla", typeof(int));
            AddNode("Oven2TempSetpoint", "ns=2;s=Tags.Eren_robin/pc_temperatura_massima_forno_colla", typeof(int));

            // System
            AddNode("SystemStatus", "ns=2;s=Tags.Eren_robin/pc_stato_macchina", typeof(int), true);
            AddNode("StartStop", "ns=2;s=Tags.Eren_robin/pc_start_stop", typeof(bool));
            AddNode("Pause", "ns=2;s=Tags.Eren_robin/pc_pausa", typeof(bool));
            AddNode("Reset", "ns=2;s=Tags.Eren_robin/pc_reset_generale", typeof(bool));

            // Lights
            AddNode("OutputPLC", "ns=2;s=Tags.Eren_robin/pc_output_plc_linea", typeof(bool[]), true);
            AddNode("RedLight", "ns=2;s=Tags.Eren_robin/pc_output_plc_linea[9]", typeof(bool), true);
            AddNode("InputPLC", "ns=2;s=Tags.Eren_robin/pc_input_plc_linea", typeof(bool[]), true);

            //Belt
            AddNode("BeltSpeed", "ns=2;s=Tags.Eren_robin/pc_velocita_nastrini", typeof(int));

            AddNode("Comunicazione", "ns=2;s=Tags.Eren_robin/pc_comunicazione_da_opc_a_plc", typeof(bool));
            AddNode("Restart", "ns=2;s=Tags.Eren_robin/pc_restart_plc", typeof(bool));

            //Manual
            AddNode("Oven1Lamps", "ns=2;s=Tags.Eren_robin/pc_accendi_lampade_forno_primer", typeof(bool));
            AddNode("Oven1Fans", "ns=2;s=Tags.Eren_robin/pc_accendi_ventilatori_forno_primer", typeof(bool));
            AddNode("Oven1Belt", "ns=2;s=Tags.Eren_robin/pc_start_nastrino_forno_primer", typeof(bool));

            AddNode("Oven2Lamps", "ns=2;s=Tags.Eren_robin/pc_accendi_lampade_forno_colla", typeof(bool));
            AddNode("Oven2Fans", "ns=2;s=Tags.Eren_robin/pc_accendi_ventilatori_forno_colla", typeof(bool));
            AddNode("Oven2Belt", "ns=2;s=Tags.Eren_robin/pc_start_nastrino_forno_colla", typeof(bool));

            AddNode("InputBelt", "ns=2;s=Tags.Eren_robin/pc_start_nastro_ingresso", typeof(bool));
            AddNode("CentralBelt", "ns=2;s=Tags.Eren_robin/pc_start_nastro_centrale", typeof(bool));
        }
    }
}