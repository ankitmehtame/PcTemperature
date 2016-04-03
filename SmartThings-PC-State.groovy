preferences
{
  input("hostaddress", "text", title: "Device Address <IP:Port>")
}

metadata
{
  definition (name: "Ankit PC State", author: "Ankit Mehta")
  {
    capability "Temperature Measurement"
    capability "Refresh"
    capability "Polling"
  }
  simulator
  { }
  tiles
  {
    standardTile("state", "device.state", inactiveLabel: true, width: 2, height: 2, canChangeIcon: true)
    {
      state "offline", label:"Offline", backgroundColor:"#ffffff", defaultState: true
      state "online", label:"Online", backgroundColor:"#79b821"
    }
    standardTile("temperature", "device.temperature", inactiveLabel: true, decoration: "flat")
    {
      state "temperature", label: '${currentValue}°C'
    }
    standardTile("refresh", "device.switch", inactiveLabel: true, decoration: "flat")
    {
      state "default", label:"", action:"refresh.refresh", icon:"st.secondary.refresh"
    }
    main("state")
    details(["state", "temperature", "refresh"])
  }
}

def parse(String description)
{
    // log.debug "parse called with ${description}"
    def msg = parseLanMessage(description)

    def status = msg.status          // => http status code of the response
    // log.debug("status ${status}")
    def json = msg.json              // => any JSON included in response body, as a data structure of lists and maps
    // log.debug("json ${json}")
    
    if(status != 200)
    {
        log.debug("status is not 200")
        updateStatus("offline")
        refreshComplete()
        return
    }
    
    def isSuccessful = json.isSuccessful
    
    if(isSuccessful == true)
    {
        log.debug("is successful")
        updateStatus("online")
        def maxcputemp = json.coreTempMax
        
        log.debug("max cpu temp ${maxcputemp}")
        sendEvent(name: "temperature", value: maxcputemp, display: true, descriptionText: device.displayName + " is ${maxcputemp} °C")
    }
    else
    {
        log.debug("temp retrieval was unsuccessful")
        updateStatus("online")
        sendEvent(name: "temperature", value: "N/A", display: true, descriptionText: device.displayName + " is N/A")
    }
    refreshComplete()
}

def poll()
{
  log.debug("poll called.")
  state.ispolling = true
  refresh()
}

def refresh()
{
  log.debug("refresh called.")
  if(state.isrefreshing == true)
  {
    log.debug("already refreshing. not proceeding.")
    return
  }
  state.isrefreshing = true
  try
  {
      log.debug "Calling ${hostaddress}"
      updateStatusBeforeRefresh()
      def cmd = myCommand()
      sendHubCommand(cmd)
  }
  catch(all)
  {
      log.debug("Error during refresh in catch all for httpget. Error: ${all}")
      updateStatus("offline")
      refreshComplete()
  }
}

def refreshComplete()
{
  state.isrefreshing = false
  log.debug("refresh complete")
  if(state.ispolling)
  {
    runIn(60 * 10, refresh)
  }
}

def myCommand() {
    def result = new physicalgraph.device.HubAction(
        method: "GET",
        path: '/api/pctemp/reading',
        headers: [
            HOST: "${hostaddress}"
        ]
    )
    return result
}

def updateStatusBeforeRefresh()
{
    runIn(15, handler)
}

def handler()
{
    if(state.isrefreshing == true)
    {
        log.debug("Looks like the call did not complete. setting state as offline")
        updateStatus("offline")
        refreshComplete()
    }
}

def updateStatus(status)
{
    if (status == "offline") { sendEvent(name: "state", value: "offline", display: true, descriptionText: device.displayName + " is offline") }
    if (status == "online") { sendEvent(name: "state", value: "online", display: true, descriptionText: device.displayName + " is online") }
}
