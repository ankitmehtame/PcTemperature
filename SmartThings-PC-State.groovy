preferences
{
  input("hostaddress", "text", title: "Device Address <IP:Port>")
}

metadata
{
  definition (name: "Ankit PC State", author: "Ankit Mehta")
  {
    capability "Refresh"
    capability "Polling"
    attribute "cputemp", "NUMBER"
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
    standardTile("refresh", "device.switch", inactiveLabel: true, decoration: "flat")
    {
      state "default", label:"", action:"refresh.refresh", icon:"st.secondary.refresh"
    }
    valueTile("cputemp", "device.cputemp", decoration: "flat")
    {
      state "cputemp", label:'CPU ${currentValue} °C'
    }
    main("state")
    details(["state", "cputemp", "refresh"])
  }
}

def parse(String description)
{
    log.debug "parse called with ${description}"
    state.counter = 0
    log.debug("counter is " + state.counter)
    def msg = parseLanMessage(description)

    def status = msg.status          // => http status code of the response
    log.debug("status ${status}")
    def json = msg.json              // => any JSON included in response body, as a data structure of lists and maps
    log.debug("json ${json}")
    
    if(status != 200)
    {
        log.debug("status is not 200")
        updateStatus("offline")
        return null
    }
    
    def isSuccessful = json.isSuccessful
    log.debug("isSuccessful? ${isSuccessful}")
    
    if(isSuccessful == true)
    {
        log.debug("is successful")
        updateStatus("online")
        def maxcputemp = json.coreTempMax
        
        log.debug("max cpu temp ${maxcputemp}")
        sendEvent(name: "cputemp", value: maxcputemp, display: true, descriptionText: "CPU temp is ${maxcputemp} °C")
    }
    else
    {
        log.debug("temp retrieval was unsuccessful")
        updateStatus("online")
        sendEvent(name: "cputemp", value: "N/A", display: true, descriptionText: "CPU temp is N/A")
    }
  return null
}

def poll()
{
  refresh()
}

def refresh()
{
  log.debug("refresh called.")
  try
  {
      return myCommand()
  }
  catch(all)
  {
        log.debug("Error during refresh in catch all for httpget. Error: ${all}")
        updateStatus("offline")
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
    log.debug "Calling ${hostaddress}"
    updateStatusBeforeRefresh()
    state.counter = 1
    return result
}

def updateStatusBeforeRefresh()
{
    runIn(15, handler)
    return null
}

def handler()
{
    if(state.counter != 0)
    {
        log.debug("Looks like the call did not complete. setting state as offline")
        updateStatus("offline")
    }
}

def updateStatus(status)
{
    if (status == "offline") { sendEvent(name: "state", value: "offline", display: true, descriptionText: device.displayName + " is offline") }
    if (status == "online") { sendEvent(name: "state", value: "online", display: true, descriptionText: device.displayName + " is online") }
}
