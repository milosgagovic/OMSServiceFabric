﻿using DMSCommon.Model;
using DMSContract;
using FTN.Common;
using FTN.ServiceContracts;
using IMSContract;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMSSCADACommon;
using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using TransactionManager.ServicecFabricClients;
using TransactionManagerContract;

namespace TransactionManager
{
    public class TransactionManager : IOMSClient
    {
        // properties for providing communication infrastructure for 2PC protocol
        List<WCFDMSTransactionClient> transactionProxys;
        List<TransactionCallback> transactionCallbacks;
        ITransaction proxyTransactionNMS;
        ITransaction proxyTransactionDMS;
        ITransactionSCADA proxyTransactionSCADA;
        TransactionCallback callBackTransactionNMS;
        TransactionCallback callBackTransactionDMS;
        TransactionCallback callBackTransactionSCADA;
        NetworkModelGDAProxy ProxyToNMSService;
        NetworkGDAServiceFabric proxyToNMServiceFabric;
        IDMSContract proxyToDispatcherDMS;
        WCFIMSClient ServiceCommunicationClient;
        ServiceFabricDMSClient proxyToDMS;

        WCFDMSTransactionClient _WCFDMSTransactionClient;
        WCFDMSTransactionClient _WCFNMSTransactionClient;
        ModelGDATMS gdaTMS;
        private SCADAClient scadaClient;
        private SCADAClient ScadaClient
        {
            get
            {
                if (scadaClient == null)
                {
                    scadaClient = new SCADAClient(new EndpointAddress("net.tcp://localhost:4000/SCADAService"));
                }
                return scadaClient;
            }
            set { scadaClient = value; }
        }

        public List<WCFDMSTransactionClient> TransactionProxys { get => transactionProxys; set => transactionProxys = value; }
        public List<TransactionCallback> TransactionCallbacks { get => transactionCallbacks; set => transactionCallbacks = value; }
        public ITransaction ProxyTransactionNMS { get => proxyTransactionNMS; set => proxyTransactionNMS = value; }
        public ITransaction ProxyTransactionDMS { get => proxyTransactionDMS; set => proxyTransactionDMS = value; }
        public ITransactionSCADA ProxyTransactionSCADA { get => proxyTransactionSCADA; set => proxyTransactionSCADA = value; }
        public TransactionCallback CallBackTransactionNMS { get => callBackTransactionNMS; set => callBackTransactionNMS = value; }
        public TransactionCallback CallBackTransactionDMS { get => callBackTransactionDMS; set => callBackTransactionDMS = value; }
        public TransactionCallback CallBackTransactionSCADA { get => callBackTransactionSCADA; set => callBackTransactionSCADA = value; }

        private IMSClient imsClient;
        private IMSClient IMSClient
        {
            get
            {
                if (imsClient == null)
                {
                    imsClient = new IMSClient(new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
                }
                return imsClient;
            }
            set { imsClient = value; }
        }

        public TransactionManager()
        {
            TransactionProxys = new List<WCFDMSTransactionClient>();
            TransactionCallbacks = new List<TransactionCallback>();

            InitializeChanels();

            gdaTMS = new ModelGDATMS();
        }
        /*
        private void InitializeChanels()
        {
            Console.WriteLine("InitializeChannels()");

            var binding = new NetTcpBinding();
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.TransactionFlow = true;
            binding.MaxReceivedMessageSize = Int32.MaxValue;

            // duplex channel for NMS transaction
            CallBackTransactionNMS = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionNMS);
            DuplexChannelFactory<ITransaction> factoryTransactionNMS = new DuplexChannelFactory<ITransaction>(CallBackTransactionNMS,
                                                         binding,
                                                         new EndpointAddress("net.tcp://localhost:8018/NetworkModelTransactionService"));
            ProxyTransactionNMS = factoryTransactionNMS.CreateChannel();
            TransactionProxys.Add(ProxyTransactionNMS);

            // duplex channel for DMS transaction
            CallBackTransactionDMS = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionDMS);
            DuplexChannelFactory<ITransaction> factoryTransactionDMS = new DuplexChannelFactory<ITransaction>(CallBackTransactionDMS,
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:8028/DMSTransactionService"));
            ProxyTransactionDMS = factoryTransactionDMS.CreateChannel();
            TransactionProxys.Add(ProxyTransactionDMS);

            // duplex channel for SCADA transaction
            CallBackTransactionSCADA = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionSCADA);
            DuplexChannelFactory<ITransactionSCADA> factoryTransactionSCADA = new DuplexChannelFactory<ITransactionSCADA>(CallBackTransactionSCADA,
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:8078/SCADATransactionService"));
            ProxyTransactionSCADA = factoryTransactionSCADA.CreateChannel();

            // client channel for SCADA 


            // client channel for DMSDispatcherService
            ChannelFactory<IDMSContract> factoryDispatcherDMS = new ChannelFactory<IDMSContract>(binding, new EndpointAddress("net.tcp://localhost:8029/DMSDispatcherService"));
            proxyToDispatcherDMS = factoryDispatcherDMS.CreateChannel();



            //ChannelFactory<IIMSContract> factoryToIMS = new ChannelFactory<IIMSContract>(binding, new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
            // proxyToIMS = factoryToIMS.CreateChannel();

            ProxyToNMSService = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
            ProxyToNMSService.Open();


            // client channel for IMS
            // factoryToIMS = new ChannelFactory<IIMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
            //IMSClient = factoryToIMS.CreateChannel();

        }*/
        private void InitializeChanels()
        {

            CallBackTransactionNMS = new TransactionCallback();
            CallBackTransactionDMS = new TransactionCallback();


            ///
            NetTcpBinding binding = new NetTcpBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            var wcfClientFactory = new WcfCommunicationClientFactory<IIMSContract>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            ServiceCommunicationClient = new WCFIMSClient(
                            wcfClientFactory,
                            new Uri("fabric:/ServiceFabricOMS/IMStatelessService"),
                            ServicePartitionKey.Singleton);



            ///
            NetTcpBinding binding2 = new NetTcpBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver2 = ServicePartitionResolver.GetDefault();

            //create calllback
            // duplex channel for DMS transaction
            //  TransactionCallback CallBackTransactionDMS2 = new TransactionCallback();
            // create a  WcfCommunicationClientFactory object.
            var wcfClientFactory2 = new WcfCommunicationClientFactory<ITransaction>
                (clientBinding: binding2, servicePartitionResolver: partitionResolver2, callback: CallBackTransactionDMS);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            _WCFDMSTransactionClient = new WCFDMSTransactionClient(
                            wcfClientFactory2,
                            new Uri("fabric:/ServiceFabricOMS/DMStatelessService"),
                            ServicePartitionKey.Singleton,
                            listenerName: "DMSTransactionService");

            TransactionProxys.Add(_WCFDMSTransactionClient);
            TransactionCallbacks.Add(CallBackTransactionDMS);
            //  ServiceCommunicationClient.InvokeWithRetry(client => client.Channel.GetCrews());
            // client channel for IMS
            // factoryToIMS = new ChannelFactory<IIMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
            //IMSClient = factoryToIMS.CreateChannel();

            NetTcpBinding binding3 = new NetTcpBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver3 = ServicePartitionResolver.GetDefault();

            //create calllback
            // duplex channel for DMS transaction
            //TransactionCallback CallBackTransactionNMS2 = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionNMS);
            // create a  WcfCommunicationClientFactory object.
            var wcfClientFactory3 = new WcfCommunicationClientFactory<ITransaction>
                (clientBinding: binding3, servicePartitionResolver: partitionResolver3, callback: CallBackTransactionNMS);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            _WCFNMSTransactionClient = new WCFDMSTransactionClient(
                            wcfClientFactory3,
                            new Uri("fabric:/ServiceFabricOMS/NMStatelessService"),
                            ServicePartitionKey.Singleton,
                            listenerName: "NMTransactionServiceEndpoint");

            TransactionProxys.Add(_WCFNMSTransactionClient);

            IServicePartitionResolver partitionResolverToDMS = ServicePartitionResolver.GetDefault();
            var wcfClientFactoryToDMS = new WcfCommunicationClientFactory<IDMSContract>
                (clientBinding: new NetTcpBinding(), servicePartitionResolver: partitionResolverToDMS);
            proxyToDMS = new ServiceFabricDMSClient(
                            wcfClientFactoryToDMS,
                            new Uri("fabric:/ServiceFabricOMS/DMStatelessService"),
                            ServicePartitionKey.Singleton,
                            listenerName: "DMSDispatcherService");



            
           
        }

        #region 2PC methods

        public void Enlist(Delta d)
        {
            Console.WriteLine("Transaction Manager calling enlist");

            foreach (WCFDMSTransactionClient svc in TransactionProxys)
            {
                svc.InvokeWithRetry(client => client.Channel.Enlist());
            }

            //            ProxyTransactionSCADA.Enlist();

            while (true)
            {
                if (TransactionCallbacks.Where(k => k.AnswerForEnlist == TransactionAnswer.Unanswered).Count() > 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    Prepare(d);
                    break;
                }
            }
        }

        public void Prepare(Delta delta)
        {
            Console.WriteLine("Transaction Manager calling prepare");
            foreach (WCFDMSTransactionClient c in TransactionProxys)
            {
                c.InvokeWithRetry(x => x.Channel.Prepare(delta));
            }
            //_WCFNMSTransactionClient.InvokeWithRetry(client => client.Channel.Prepare(delta));
            //Thread.Sleep(100);
            //_WCFDMSTransactionClient.InvokeWithRetry(client => client.Channel.Prepare(delta));

            //  proxyTransactionNMS.Prepare(delta);
            // ScadaDelta scadaDelta = GetDeltaForSCADA(delta);
            do
            {
                Thread.Sleep(50);
            } while (CallBackTransactionNMS.AnswerForPrepare.Equals(TransactionAnswer.Unanswered));

            if (CallBackTransactionNMS.AnswerForPrepare.Equals(TransactionAnswer.Unprepared))
            {
                Rollback();
            }
            else
            {
                /*
                 ako ne uspe nms, nece se ni pozvati dms
                 */

                // ovde samo dms poziva
                //    TransactionProxys.Where(u => !u.Equals(ProxyTransactionNMS)).ToList().ForEach(x => x.Prepare(delta));

                ///  ProxyTransactionSCADA.Prepare(scadaDelta);



                while (true)
                {
                    if (TransactionCallbacks.Where(k => k.AnswerForPrepare == TransactionAnswer.Unanswered).Count() > 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    else if (TransactionCallbacks.Where(u => u.AnswerForPrepare == TransactionAnswer.Unprepared).Count() > 0)
                    {
                        Rollback();
                        break;
                    }

                    Commit();
                    break;
                }
            }
        }

        private void Commit()
        {
            Console.WriteLine("Transaction Manager calling commit");
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Commit();
            }

         //   ProxyTransactionSCADA.Commit();
        }

        public void Rollback()
        {
            Console.WriteLine("Transaction Manager calling rollback");
            foreach (WCFDMSTransactionClient c in TransactionProxys)
            {
                c.InvokeWithRetry(x => x.Channel.Rollback());
            }
            //foreach (ITransaction svc in TransactionProxys)
            //{
            //    svc.Rollback();
            //}
            //ProxyTransactionSCADA.Rollback();
        }

        #endregion

        #region IOMSClient CIMAdapter Methods

        // so, in order for network to be initialized, UpdateSystem must be called first

        /// <summary>
        /// Called by ModelLabs(CIMAdapter) when Static data changes
        /// </summary>
        /// <param name="d">Delta</param>
        /// <returns></returns>
        public bool UpdateSystem(Delta d)
        {
            Console.WriteLine("Update System started." + d.Id);
            Enlist(d);
            //  Prepare(d);
            return true;
        }

        public void ClearNMSDB()
        {
            using (NMSAdoNet ctx = new NMSAdoNet())
            {
                var tableNames = ctx.Database.SqlQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%Migration%'").ToList();
                foreach (var tableName in tableNames)
                {
                    ctx.Database.ExecuteSqlCommand(string.Format("DELETE FROM {0}", tableName));
                }

                ctx.SaveChanges();
            }
        }

        #endregion

        #region  IOMSClient DispatcherApp Methods

        public TMSAnswerToClient GetNetwork()
        {
            List<Element> listOfDMSElement = proxyToDMS.InvokeWithRetry(client => client.Channel.GetAllElements());
            // ako se ne podignu svi servisi na DMSu, ovde pada
            //   List<Element> listOfDMSElement = proxyToDispatcherDMS.GetAllElements();

            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            List<ResourceDescription> descMeas = new List<ResourceDescription>();
            //using (NMSAdoNet ctx = new NMSAdoNet())
            //{
            //    List<PropertyValue> propValues = (List<PropertyValue>)ctx.PropertyValue.ToList();
            //    List<Property> properties = ctx.Property.ToList();
            //    properties.ForEach(x => x.PropertyValue = ctx.PropertyValue.Where(y => y.Id == x.IdDB).FirstOrDefault());

            //    if (properties.Count > 0)
            //    {
            //        foreach (ResourceDescription rd in ctx.ResourceDescription)
            //        {
            //            List<Property> rdProp = (List<Property>)properties.Where(x => x.ResourceDescription_Id == rd.IdDb).ToList();
            //            ResourceDescription res = new ResourceDescription(rd.Id, rdProp);
            //            resourceDescriptionFromNMS.Add(res);
            //        }
            //    }
            //}
            //for (int i=0; i < 74; i++)
            //{
            //    resourceDescriptionFromNMS.Add(new ResourceDescription());
            //}

            // List<ResourceDescription> list = proxyToNMServiceFabric.InvokeWithRetry(client => client.Channel.GetExtentValues(ModelCode.BREAKER));
            gdaTMS.GetExtentValues(ModelCode.BREAKER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.CONNECTNODE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGCONSUMER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGSOURCE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ACLINESEGMENT).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.DISCRETE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ANALOG).ForEach(u => resourceDescriptionFromNMS.Add(u));

            int GraphDeep = proxyToDMS.InvokeWithRetry(client => client.Channel.GetNetworkDepth());

            //try
            //{
            //    Command c = MappingEngineTransactionManager.Instance.MappCommand(TypeOfSCADACommand.ReadAll, "", 0, 0);

            //    //bool isScadaAvailable = false;
            //    //do
            //    //{
            //    //    Console.WriteLine("scada not available");
            //    //    try
            //    //    {
            //    //        if (ScadaClient.State == CommunicationState.Created)
            //    //        {
            //    //            ScadaClient.Open();
            //    //        }

            //    //        isScadaAvailable = ScadaClient.Ping();
            //    //    }
            //    //    catch (Exception e)
            //    //    {
            //    //        //Console.WriteLine(e);
            //    //        Console.WriteLine("InitializeNetwork() -> SCADA is not available yet.");
            //    //        if (ScadaClient.State == CommunicationState.Faulted)
            //    //            ScadaClient = new SCADAClient(new EndpointAddress("net.tcp://localhost:4000/SCADAService"));
            //    //    }
            //    //    Thread.Sleep(500);
            //    //} while (!isScadaAvailable);



            //    do
            //    {
            //        try
            //        {
            //            if (ScadaClient.State == CommunicationState.Created)
            //            {
            //                ScadaClient.Open();
            //            }

            //            if (ScadaClient.Ping())
            //                break;
            //        }
            //        catch (Exception e)
            //        {
            //            //Console.WriteLine(e);
            //            Console.WriteLine("GetNetwork() -> SCADA is not available yet.");
            //            if (ScadaClient.State == CommunicationState.Faulted)
            //                ScadaClient = new SCADAClient(new EndpointAddress("net.tcp://localhost:4000/SCADAService"));
            //        }
            //        Thread.Sleep(500);
            //    } while (true);
            //    Console.WriteLine("GetNetwork() -> SCADA is available.");


            //    Response r = ScadaClient.ExecuteCommand(c);
            //    descMeas = MappingEngineTransactionManager.Instance.MappResult(r);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}

            //bool isImsAvailable = false;
            //do
            //{
            //    try
            //    {
            //        if (IMSClient.State == CommunicationState.Created)
            //        {
            //            IMSClient.Open();
            //        }

            //        isImsAvailable = IMSClient.Ping();
            //    }
            //    catch (Exception e)
            //    {
            //        //Console.WriteLine(e);
            //        Console.WriteLine("GetNetwork() -> IMS is not available yet.");
            //        if (IMSClient.State == CommunicationState.Faulted)
            //            IMSClient = new IMSClient(new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
            //    }
            //    Thread.Sleep(2000);
            //} while (!isImsAvailable);

           // var crews = IMSClient.GetCrews();
            var crews = ServiceCommunicationClient.InvokeWithRetry(client => client.Channel.GetCrews());
            var incidentReports = ServiceCommunicationClient.InvokeWithRetry(client => client.Channel.GetAllReports());

            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, listOfDMSElement, GraphDeep, descMeas, crews, incidentReports);
            return answer;
        }

        public void SendCommandToSCADA(TypeOfSCADACommand command, string mrid, CommandTypes commandtype, float value)
        {
            try
            {
                Command c = MappingEngineTransactionManager.Instance.MappCommand(command, mrid, commandtype, value);

                // to do: ping
                Response r = scadaClient.ExecuteCommand(c);
                //Response r = SCADAClientInstance.ExecuteCommand(c);

            }
            catch (Exception e)
            { }
        }

        public void SendCrew(IncidentReport report)
        {
            proxyToDispatcherDMS.SendCrewToDms(report);
            return;
        }

        // currently unused
        public bool IsNetworkAvailable()
        {
            bool retVal = false;
            try
            {
                retVal = proxyToDispatcherDMS.IsNetworkAvailable();
            }
            catch (System.ServiceModel.EndpointNotFoundException e)
            {
                //Console.WriteLine("DMSDispatcher is not available yet.");
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {

            }

            return retVal;
        }

        private ScadaDelta GetDeltaForSCADA(Delta d)
        {
            // zasto je ovo bitno, da ima measurement direction?? 
            // po tome odvajas measuremente od ostatka?
            List<ResourceDescription> rescDesc = d.InsertOperations.Where(u => u.ContainsProperty(ModelCode.MEASUREMENT_DIRECTION)).ToList();
            ScadaDelta scadaDelta = new ScadaDelta();

            foreach (ResourceDescription rd in rescDesc)
            {
                ScadaElement element = new ScadaElement();
                if (rd.ContainsProperty(ModelCode.MEASUREMENT_TYPE))
                {
                    string type = rd.GetProperty(ModelCode.MEASUREMENT_TYPE).ToString();
                    if (type == "Analog")
                    {
                        element.Type = DeviceTypes.ANALOG;                       
                        element.UnitSymbol = ((UnitSymbol)rd.GetProperty(ModelCode.MEASUREMENT_UNITSYMB).AsEnum()).ToString();
                        element.WorkPoint = rd.GetProperty(ModelCode.ANALOG_NORMVAL).AsFloat();
                    }
                    else if (type == "Discrete")
                    {
                        element.Type = DeviceTypes.DIGITAL;
                    }
                }

                element.ValidCommands = new List<CommandTypes>() { CommandTypes.CLOSE, CommandTypes.OPEN };
                element.ValidStates = new List<OMSSCADACommon.States>() { OMSSCADACommon.States.CLOSED, OMSSCADACommon.States.OPENED };

                if (rd.ContainsProperty(ModelCode.IDOBJ_MRID))
                {
                    //element.Name = rd.GetProperty(ModelCode.IDOBJ_NAME).ToString();
                    element.Name = rd.GetProperty(ModelCode.IDOBJ_MRID).ToString();
                }
                scadaDelta.InsertOps.Add(element);
            }
            return scadaDelta;
        }

        #endregion

        // da li se ove metode ikada pozivaju?  Onaj console1 ne koristimo?

        // SVUDA PRVO PROVERITI DA LI JE IMS DOSTUPAN? 
        // tj naprviti metodu koja to radi
        #region Unused? check this!!!

        public void GetNetworkWithOutParam(out List<Element> DMSElements, out List<ResourceDescription> resourceDescriptions, out int GraphDeep)
        {
            //List<Element> listOfDMSElement = proxyToDMS.InvokeWithRetry(client => client.Channel.GetAllElements());
             List<Element> listOfDMSElement = new List<Element>();//proxyToDMS.GetAllElements();
            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            List<ACLine> acList = proxyToDispatcherDMS.GetAllACLines();
            List<Node> nodeList = proxyToDispatcherDMS.GetAllNodes();
            List<Source> sourceList = proxyToDispatcherDMS.GetAllSource();
            List<Switch> switchList = proxyToDispatcherDMS.GetAllSwitches();
            List<Consumer> consumerList = proxyToDispatcherDMS.GetAllConsumers();

            acList.ForEach(u => listOfDMSElement.Add(u));
            nodeList.ForEach(u => listOfDMSElement.Add(u));
            sourceList.ForEach(u => listOfDMSElement.Add(u));
            switchList.ForEach(u => listOfDMSElement.Add(u));
            consumerList.ForEach(u => listOfDMSElement.Add(u));

            gdaTMS.GetExtentValues(ModelCode.BREAKER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.CONNECTNODE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGCONSUMER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGSOURCE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ACLINESEGMENT).ForEach(u => resourceDescriptionFromNMS.Add(u));
            GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();
            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, null, GraphDeep, null, null, null);
            resourceDescriptions = resourceDescriptionFromNMS;
            DMSElements = listOfDMSElement;
            GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();

            // return resourceDescriptionFromNMS;
        }

        //public void AddReport(string mrID, DateTime time, string state)
        //{
        //    IMSClient.AddReport(mrID, time, state);
        //}

        public List<List<ElementStateReport>> GetElementStateReportsForMrID(string mrID)
        {
            return IMSClient.GetElementStateReportsForMrID(mrID);
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetElementStateReportsForSpecificTimeInterval(startTime, endTime);
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(mrID, startTime, endTime);
        }

        public void SendCrew(string mrid)
        {
            throw new NotImplementedException();
        }

        public List<Crew> GetCrews()
        {
            return ServiceCommunicationClient.InvokeWithRetry(client => client.Channel.GetCrews());
           // return IMSClient.GetCrews();
        }

        //public void SendCrew(string mrid)
        //{
        //    proxyToDispatcherDMS.SendCrewToDms(mrid);
        //    return;
        //}

        public bool AddCrew(Crew crew)
        {
            return IMSClient.AddCrew(crew);
        }

        public void AddReport(IncidentReport report)
        {
            IMSClient.AddReport(report);
        }

        public List<IncidentReport> GetAllReports()
        {
            return IMSClient.GetAllReports();
        }

        public List<List<IncidentReport>> GetReportsForMrID(string mrID)
        {
            return IMSClient.GetReportsForMrID(mrID);
        }

        public List<IncidentReport> GetReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetReportsForSpecificTimeInterval(startTime, endTime);
        }

        public List<IncidentReport> GetReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetReportsForSpecificMrIDAndSpecificTimeInterval(mrID, startTime, endTime);
        }

        public List<ElementStateReport> GetAllElementStateReports()
        {
            return IMSClient.GetAllElementStateReports();
        }

        public List<List<IncidentReport>> GetReportsForSpecificDateSortByBreaker(List<string> mrids, DateTime date)
        {
            return IMSClient.GetReportsForSpecificDateSortByBreaker(mrids, date);
        }

        public List<List<IncidentReport>> GetAllReportsSortByBreaker(List<string> mrids)
        {
            return IMSClient.GetAllReportsSortByBreaker(mrids);
        }

       
        #endregion
    }
}